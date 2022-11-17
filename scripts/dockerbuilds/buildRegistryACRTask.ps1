az acr task create `
    --registry aasapiimages `
    --name buildregistryV30RC02 `
    --image aas-registry-server:latest `
    --context https://github.com/JMayrbaeurl/opendigitaltwins-aas-azureservices.git#V30RC02_Registry `
    --file src/aas-api-webapp-registry/Dockerfile `
    --git-access-token <GITHUB_PERSONALTOKEN>