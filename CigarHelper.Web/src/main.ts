import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./assets/main.css";
import PrimeVue from "primevue/config";
import Aura from "@primeuix/themes/aura";
import ConfirmationService from "primevue/confirmationservice";
import ToastService from "primevue/toastservice";

const app = createApp(App);

app.use(router);
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      cssLayer: {
        name: "primevue",
        order: "theme, base, primevue",
      },
    },
  },
  ptOptions: {
    mergeSections: true,
    mergeProps: true,
  },
});
app.use(ConfirmationService);
app.use(ToastService);

app.mount("#app"); 