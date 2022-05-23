# OpenArtcoded Dropbox-sync

## Informations




## How to install

```yaml
version: '3.4'

services:
  dropboxsync:
    image: nbittich/dropbox-sync
    restart: always
    depends_on:
      - artemis
      - keycloak
      - api-backend
    networks:
        - artcoded
    environment:
        AMQP_USERNAME: root
        AMQP_PASSWORD: root
        AMQP_HOST: artemis
        AMQP_PORT: 61616
        DROPBOX_API_KEY: awqnjyjxwno5z29
        DROPBOX_API_SECRET: h5t8m7rvvg5a55x
        DROPBOX_CODE: PgYD8ACqPWcAAAAAAAAATtMVR0SsNdK5hp1f-GHBl7M
        DROPBOX_CONFIG_PATH: "/app/config"
        API_BACKEND_URL: http://api-backend
        API_BACKEND_ID: service-account-download
        API_CLIENT_SECRET: duzp0kzwDHSS2nSO46P3GBGsNnQbx5L3
        API_TOKEN_URL: http://keycloak:8080/realms/Artcoded/protocol/openid-connect/token
        FILE_DOWNLOAD_DIR: /data
        DROPBOX_APPDATA_PATH: /db
    volumes:
        - ./config/dropbox-sync:/app/config
        - ./data/dropbox-data:/data
        - ./data/dropbox-db:/db
```
