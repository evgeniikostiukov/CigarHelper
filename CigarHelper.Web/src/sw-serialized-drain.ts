/**
 * Очередь асинхронных задач для Service Worker: следующая стартует после завершения предыдущей
 * (в т.ч. после отклонения промиса — цепочка не рвётся).
 */
let drainSerial = Promise.resolve();

export function runSerializedDrain(fn: () => Promise<void>): Promise<void> {
  const p = drainSerial.then(() => fn());
  drainSerial = p.then(
    () => undefined,
    () => undefined,
  );
  return p;
}

/** Только для Vitest — сброс замыкания между тестами. */
export function resetSerializedDrainStateForTest(): void {
  drainSerial = Promise.resolve();
}
