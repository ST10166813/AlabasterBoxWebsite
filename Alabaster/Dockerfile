# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything and publish in Release mode
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Stage 2: Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Render expects apps to listen on PORT env var
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

# 👇 Replace YourAppName with your actual DLL name
ENTRYPOINT ["dotnet", "Alabaster.dll"]