#!/bin/bash
if [[ "${BUILD_SOURCEBRANCHNAME}" == "dev" || "${BUILD_SOURCEBRANCHNAME}" == "staging" || "${BUILD_SOURCEBRANCHNAME}" == "master" ]]; then
mv $(pwd)/src/environments/environment.prod.ts.template $(pwd)/src/environments/environment.prod.ts
    export githash=$(git rev-parse HEAD)
    export versionstring="${!version}.$BUILD_BUILDID-$githash" #using https://mywiki.wooledge.org/BashFAQ/006#Indirection for dynamic variables
    sed -i 's,#{identityServer:authorityUrl}#,'"${authorityUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{identityServer:clientId}#/'"${clientId}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{identityServer:scopes}#/'"${scopes}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{identityServer:logLevel}#/'"${loglevel}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{apiBaseUrl}#,'"${apiurl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{smartyStreetsAutoCompleteKey}#,'"${smartyStreetsAutoCompleteKey}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{version}#/'"$versionstring"'/g' $(pwd)/src/environments/environment.prod.ts #use s, when you want to escape anything like url else use s/
    sed -i 's,#{identityServer:registrationUrl}#,'"${registrationUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{availableThemes:0:name}#/'"${availableThemes_0_name}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:0:logoUrl}#,'"${availableThemes_0_logoUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:0:cssUrl}#,'"${availableThemes_0_cssUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:0:faviconUrl}#,'"${availableThemes_0_faviconUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{availableThemes:0:isEnabled}#/'"${availableThemes_0_isEnabled}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{availableThemes:1:name}#/'"${availableThemes_1_name}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:1:logoUrl}#,'"${availableThemes_1_logoUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:1:cssUrl}#,'"${availableThemes_1_cssUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's,#{availableThemes:1:faviconUrl}#,'"${availableThemes_1_faviconUrl}"',g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{availableThemes:1:isEnabled}#/'"${availableThemes_1_isEnabled}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:colorScheme}#/'"${miniProfiler_colorScheme}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:maxTraces}#/'"${miniProfiler_maxTraces}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:position}#/'"${miniProfiler_position}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:toggleShortcut}#/'"${miniProfiler_toggleShortcut}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:showControls}#/'"${miniProfiler_showControls}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:enabled}#/'"${miniProfiler_enabled}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{miniProfiler:enableGlobalMethod}#/'"${miniProfiler_enableGlobalMethod}"'/g' $(pwd)/src/environments/environment.prod.ts
    sed -i 's/#{defaultFinanceWaitingTime}#/'"${defaultFinanceWaitingTime}"'/g' $(pwd)/src/environments/environment.prod.ts
    cat $(pwd)/src/environments/environment.prod.ts
    npm run build-prod
else
    npm run build -- --aot
fi