import { defineComponent, h } from 'vue';
import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

/** Проверка связки Vitest + happy-dom + @vue/test-utils перед компонентными тестами. */
const Stub = defineComponent({
  name: 'VueTestUtilsSmoke',
  setup(_, { attrs }) {
    return () => h('span', { ...attrs }, 'vue-test-utils-ok');
  },
});

describe('@vue/test-utils', () => {
  it('монтирует компонент и видит разметку', () => {
    const w = mount(Stub, { attrs: { 'data-testid': 'vtu-smoke' } });
    expect(w.get('[data-testid="vtu-smoke"]').text()).toBe('vue-test-utils-ok');
  });
});
