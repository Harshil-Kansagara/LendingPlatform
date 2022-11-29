

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  identityServer: {
    authorityUrl: 'https://auth.jamoon.net/auth/realms/local',
    clientId: 'Customer-Frontend-Local',
    scopes: 'openid profile offline_access',
    logLevel: 0,
    registrationUrl: 'https://auth.jamoon.net/auth/realms/local/protocol/openid-connect/registrations'
  },
  apiBaseUrl: 'https://localhost:44311',
  version: '1.0.local',
  smartyStreetsAutoCompleteKey: '29928573398204698',
  availableThemes: [
    {
      name: 'Default',
      logoUrl: 'assets/images/sample bank.svg',
      cssUrl: 'assets/styles/custom-variables.css',
      faviconUrl: 'assets/images/favicon.png',
      isEnabled:true
    },
    {
      name: 'VantageWest',
      logoUrl: 'https://harshiltest.s3.us-east-2.amazonaws.com/Themes/Vantage.png',
      cssUrl: 'https://harshiltest.s3.us-east-2.amazonaws.com/Themes/custom-variables(for+vantage+west).css',
      faviconUrl: 'https://harshiltest.s3.us-east-2.amazonaws.com/Themes/vantage_icon.png',
      isEnabled: false
    }
  ],
  miniProfiler: {
    colorScheme: 'Auto', /** The theme can be 'Light' | 'Dark' | 'Auto'. Defaults to Auto. */
    maxTraces: 15, /** The maximum number of traces shown in the panel. */
    position: 'BottomLeft', /** The position of the panel can be 'Left' | 'Right' | 'BottomLeft' | 'BottomRight'. Defaults to Left. */
    toggleShortcut: 'Alt+M', /** The shortcut for toggling the panel. Defaults to 'Alt+M'. */
    showControls: false, /** Whether or not the controls should be shown. */
    enabled: false, /** Whether or not MiniProfiler is enabled. */
    enableGlobalMethod: true /** Whether or not a "enableMiniProfiler" method should be added to the window object. Defaults to true. */
  },
  defaultFinanceWaitingTime: 30
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
