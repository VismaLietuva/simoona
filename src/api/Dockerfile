# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching on restore
COPY Shrooms.Authentification/Shrooms.Authentification.csproj Shrooms.Authentification/
COPY Shrooms.Contracts/Shrooms.Contracts.csproj Shrooms.Contracts/
COPY Shrooms.Contracts.DataTransferObjects/Shrooms.Contracts.DataTransferObjects.csproj Shrooms.Contracts.DataTransferObjects/
COPY Shrooms.DataLayer/Shrooms.DataLayer.csproj Shrooms.DataLayer/
COPY Shrooms.DataLayer.EntityModels/Shrooms.DataLayer.EntityModels.csproj Shrooms.DataLayer.EntityModels/
COPY Shrooms.Domain/Shrooms.Domain.csproj Shrooms.Domain/
COPY Shrooms.Domain.Exceptions/Shrooms.Domain.Exceptions.csproj Shrooms.Domain.Exceptions/
COPY Shrooms.Domain.ServiceValidators/Shrooms.Domain.ServiceValidators.csproj Shrooms.Domain.ServiceValidators/
COPY Shrooms.Infrastructure/Shrooms.Infrastructure.csproj Shrooms.Infrastructure/
COPY Shrooms.IoC/Shrooms.IoC.csproj Shrooms.IoC/
COPY Shrooms.Premium/Shrooms.Premium.csproj Shrooms.Premium/
COPY Shrooms.Presentation.Api/Shrooms.Presentation.Api.csproj Shrooms.Presentation.Api/
COPY Shrooms.Presentation.Common/Shrooms.Presentation.Common.csproj Shrooms.Presentation.Common/
COPY Shrooms.Presentation.ModelMappings/Shrooms.Presentation.ModelMappings.csproj Shrooms.Presentation.ModelMappings/
COPY Shrooms.Presentation.WebViewModels/Shrooms.Presentation.WebViewModels.csproj Shrooms.Presentation.WebViewModels/
COPY Shrooms.Resources/Shrooms.Resources.csproj Shrooms.Resources/
COPY stylecop.json .

RUN dotnet restore Shrooms.Presentation.Api/Shrooms.Presentation.Api.csproj -p:Configuration=Release

# Copy all source and publish
COPY . .
RUN dotnet publish Shrooms.Presentation.Api/Shrooms.Presentation.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Shrooms.Presentation.Api.dll"]
