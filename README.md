# earthfusion-backend

[![Build Status](https://travis-ci.com/xiongnemo/earthfusion-backend.svg?token=eVKLnmA7cJigiwqAoaHv&branch=master)](https://travis-ci.com/xiongnemo/earthfusion-backend)
[![CodeFactor](https://www.codefactor.io/repository/github/xiongnemo/earthfusion-backend/badge?s=1af991d5c9acc63fe503f7d96cfbbf5a3c5f048c)](https://www.codefactor.io/repository/github/xiongnemo/earthfusion-backend)

CSharp webapi for EarthFusion, project of Database course design, Tongji SSE, Jul 2020

## Init

```bash
dotnet add package Oracle.ManagedDataAccess.Core --version 2.19.70
dotnet add package Sendgrid --version 9.18.0
dotnet add package StackExchange.Redis --version 2.1.58
dotnet add package LettuceEncrypt --version 1.0.0 # if you want to automatically obtain Let's Encrypt's certs
```

## Run

```bash
export EARTH_FUSION_EMAIL_API_KEY="***" # Sendgrid API Key
export EARTH_FUSION_SPATIAL_DB_USERNAME="***" # Oracle
export EARTH_FUSION_SPATIAL_DB_PASSWORD="***" # Oracle
export EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME="***" # Oracle
export EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD="***" # Oracle
dotnet build
./bin/Debug/netcoreapp3.1/earthfusion-backend
```

* It's recommended to create a script.

## Dev Logs

### 2020/7/12

- [X] Pull first *** wkt rows with given TableName.

### 2020/7/21

- [X] HTTPS support.

### 2020/8/2

- [X] Response compression, enabled over HTTPS: [source](https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-3.1)

Todo:

- [ ] Use config file for auto configuration.
