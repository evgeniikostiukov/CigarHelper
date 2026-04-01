import { beforeEach, describe, expect, it } from 'vitest';
import { resetSerializedDrainStateForTest, runSerializedDrain } from './sw-serialized-drain';

beforeEach(() => {
  resetSerializedDrainStateForTest();
});

describe('runSerializedDrain', () => {
  it('выполняет параллельно запланированные задачи строго по очереди', async () => {
    const order: number[] = [];
    const slow = runSerializedDrain(async () => {
      order.push(1);
      await new Promise((r) => {
        setTimeout(r, 15);
      });
    });
    const fast = runSerializedDrain(async () => {
      order.push(2);
    });
    await Promise.all([slow, fast]);
    expect(order).toEqual([1, 2]);
  });

  it('после отклонения задачи следующая всё равно выполняется', async () => {
    const order: string[] = [];
    await runSerializedDrain(async () => {
      order.push('a');
      throw new Error('fail');
    }).catch(() => undefined);
    await runSerializedDrain(async () => {
      order.push('b');
    });
    expect(order).toEqual(['a', 'b']);
  });

  it('не запускает вторую задачу до завершения первой (включая await внутри)', async () => {
    let phase = 0;
    const p1 = runSerializedDrain(async () => {
      expect(phase).toBe(0);
      phase = 1;
      await new Promise((r) => {
        setTimeout(r, 20);
      });
      phase = 2;
    });
    const p2 = runSerializedDrain(async () => {
      expect(phase).toBe(2);
      phase = 3;
    });
    await Promise.all([p1, p2]);
    expect(phase).toBe(3);
  });

  it('пробрасывает отклонение вызывающему await на этой задаче', async () => {
    await expect(
      runSerializedDrain(async () => {
        throw new Error('x');
      }),
    ).rejects.toThrow('x');
  });
});
