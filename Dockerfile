FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/PrimeCalcApi.WebService/*.csproj ./src/PrimeCalcApi.WebService/
RUN dotnet restore

# copy everything else and build app
COPY src/PrimeCalcApi.WebService/. ./src/PrimeCalcApi.WebService/
WORKDIR /app/src/PrimeCalcApi.WebService
RUN dotnet publish -c Release -o out -r linux-x64

FROM microsoft/dotnet:2.1-runtime-deps AS runtime
WORKDIR /app
COPY --from=build /app/src/PrimeCalcApi.WebService/out ./
ENTRYPOINT ["./PrimeCalcApi.WebService"]