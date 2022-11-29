#!/bin/bash
if [[ "${BUILD_SOURCEBRANCHNAME}" == "dev" || "${BUILD_SOURCEBRANCHNAME}" == "staging" || "${BUILD_SOURCEBRANCHNAME}" == "master" ]]; then
    export bucket="customer-front-end-${BUILD_SOURCEBRANCHNAME}"
    export expiration='public, max-age=31536000'
    cd ./dist/ClientApp/
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "application/javascript" --cache-control "$expiration" --exclude "*" --include "*.js"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "application/font-sfnt" --cache-control "$expiration" --exclude "*" --include "*.ttf"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "application/vnd.ms-fontobject" --cache-control "$expiration" --exclude "*" --include "*.eot"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "application/font-woff" --cache-control "$expiration" --exclude "*" --include "*.woff"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "image/svg+xml" --cache-control "$expiration" --exclude "*" --include "*.svg"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "image/png" --cache-control "$expiration" --exclude "*" --include "*.png"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "text/css" --cache-control "$expiration" --exclude "*" --include "*.css"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "text/html" --cache-control "no-cache" --exclude "*" --include "*.html"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "image/x-icon" --cache-control "$expiration" --exclude "*" --include "*.ico"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "image/jpg" --cache-control "$expiration" --exclude "*" --include "*.jpg"
    aws s3 cp . s3://$bucket/ --recursive --metadata-directive "REPLACE" --content-type "application/json" --cache-control "$expiration" --exclude "*" --include "*.json"
fi
