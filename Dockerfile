# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# คัดลอกไฟล์ .sln และ .csproj
COPY PageDemo1.sln .
COPY PageDemo1/*.csproj PageDemo1/

# Restore dependencies
RUN dotnet restore PageDemo1.sln

# คัดลอก source code ทั้งหมด
COPY . .

# Build และ publish
WORKDIR /src/PageDemo1
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# คัดลอกไฟล์จาก build stage
COPY --from=build /app/publish .

# เปิดพอร์ต (ปรับตามโปรเจคบูม)
EXPOSE 8080

# รันแอป
ENTRYPOINT ["dotnet", "PageDemo1.dll"]
