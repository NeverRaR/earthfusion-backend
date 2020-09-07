# earthfusion-backend

[![Build Status](https://travis-ci.com/xiongnemo/earthfusion-backend.svg?token=eVKLnmA7cJigiwqAoaHv&branch=master)](https://travis-ci.com/xiongnemo/earthfusion-backend)
[![CodeFactor](https://www.codefactor.io/repository/github/xiongnemo/earthfusion-backend/badge?s=1af991d5c9acc63fe503f7d96cfbbf5a3c5f048c)](https://www.codefactor.io/repository/github/xiongnemo/earthfusion-backend)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fxiongnemo%2Fearthfusion-backend.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fxiongnemo%2Fearthfusion-backend.svg?ref=badge_shield)

CSharp webapi for EarthFusion, project of Database course design, Tongji SSE, Jul 2020

## Init

```bash
dotnet add package Oracle.ManagedDataAccess.Core --version 2.19.70
dotnet add package Sendgrid --version 9.18.0
dotnet add package StackExchange.Redis --version 2.1.58
dotnet add package LettuceEncrypt --version 1.0.0 # if you want to automatically obtain Let's Encrypt's certs
```

## Setup

Put your https certificate(certificate.pfx) under project root.

Consult your DB Administrator for DB conference.

I assume your spatial admin's account is `spatial_admin`, and your geom data is in `nemo`'s schema.

```sql
CREATE TABLE earthfusion_users(
            user_id NUMBER,
            user_name VARCHAR2(50),
            user_email VARCHAR2(50),
            user_password_hashed VARCHAR2(66),
            user_status VARCHAR2(50),
            user_role VARCHAR2(50),
            PRIMARY KEY(user_id)
            );
```


## Run

Firstly, create an `ini` file under project root.

Contents for `config.ini`:

```ini
EARTH_FUSION_EMAIL_API_KEY=***
EARTH_FUSION_SPATIAL_DB_USERNAME=***
EARTH_FUSION_SPATIAL_DB_PASSWORD=***
EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME=***
EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD=***
EARTH_FUSION_LISTEN_HTTP_PORT=5000
EARTH_FUSION_LISTEN_HTTPS_PORT=5001
```

Build and run:

```bash
#!/bin/bash
#
# no longer need this as we has ini config
# export EARTH_FUSION_EMAIL_API_KEY="***" # Sendgrid API Key
# export EARTH_FUSION_SPATIAL_DB_USERNAME="***" # Oracle
# export EARTH_FUSION_SPATIAL_DB_PASSWORD="***" # Oracle
# export EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME="***" # Oracle
# export EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD="***" # Oracle
# 
cp ./config.ini ./bin/Debug/netcoreapp3.1/
dotnet build
./bin/Debug/netcoreapp3.1/earthfusion-backend
```

* It's recommended to create a script.

## Remote control over HTTPS

Create a `remote_web_deployment_config.json` file for self defined port number to listen on.

```json
{
    "earthfusion": {
        "remote_web_deployment": {
            "port_num": ****
        }
    }
}
```

By default(no config file present), it listens on `6000`.

```bash
python3 ./remote_web_deployment.py
```

API Endpoint:

```url
https://[your_machine]:[port_num]/api/earthfusion_ctl/stop_pull_start
```

Make sure to put an `rebuild_and_start.sh` under project root.

## Dev Logs

### 2020/7/21

- [X] HTTPS support.

### 2020/8/2

- [X] Response compression, enabled over HTTPS: [source](https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-3.1)

### 2020/8/19

- [X] Use ini file for config

### 2020/9/6

- [X] Remote control and deploy over HTTPS
