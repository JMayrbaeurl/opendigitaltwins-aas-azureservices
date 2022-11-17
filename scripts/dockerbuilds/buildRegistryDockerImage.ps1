# Make sure to run this script from the root folder of the solution

az acr build -f .\src\aas-api-webapp-registry\Dockerfile --registry aasapiimages --image aas-registry-server:latest .