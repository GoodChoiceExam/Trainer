FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FitLife.Trainer.slnx .
COPY FitLife.Trainer.Api/FitLife.Trainer.Api.csproj FitLife.Trainer.Api/
COPY FitLife.Trainer.Tests/FitLife.Trainer.Tests.csproj FitLife.Trainer.Tests/
RUN dotnet restore FitLife.Trainer.slnx

COPY . .
RUN dotnet test FitLife.Trainer.Tests/FitLife.Trainer.Tests.csproj --no-restore
RUN dotnet publish FitLife.Trainer.Api/FitLife.Trainer.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FitLife.Trainer.Api.dll"]
