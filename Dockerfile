# -------------------------
# Build aşaması
# -------------------------
     FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
     WORKDIR /src
     
     # Proje dosyasını kopyala ve restore et
     COPY Api/Api.csproj ./Api/
     WORKDIR /src/Api
     RUN dotnet restore
     
     # Tüm kaynak dosyaları kopyala ve build et
     COPY Api/. ./
     RUN dotnet publish -c Release -o /app/publish
     
     # -------------------------
     # Runtime aşaması
     # -------------------------
     FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
     WORKDIR /app
     
     # Yayınlanan dosyaları kopyala
     COPY --from=build /app/publish ./
     
     # Ortam değişkenleri ve port ayarı
     ENV ASPNETCORE_URLS=http://0.0.0.0:8080
     ENV ASPNETCORE_ENVIRONMENT=Production
     EXPOSE 8080
     
     # API başlat
     ENTRYPOINT ["dotnet", "Api.dll"]