import { createAction } from "../state.helpers";

export const hideCookieConsent = createAction(
  "[Common] Hide cookie consent message"
);

export const showCookieConsent = createAction(
  "[Common] Show cookie consent message"
);

export const toggleNavigationGroup = createAction<{ readonly path: string }>(
  "[Common] Toggle navigation group"
);
