# OpenArtcoded Dropbox-sync

## Informations

* Langage : C#
* SDK : .NET 6

## Installation

### Dropbox Configuration

Before touching the app, you will need to create an application in Dropbox and retrieve some informations. Go on [Dropbox Developper site](https://www.dropbox.com/developers/).
On the upper right corner, select **App console**. Create a new app.

#### Choose an API

Select **Scoped access**, normally there is no other choice, but it might change in the future

![API Choice](https://user-images.githubusercontent.com/56565073/169828520-4f602a92-58ca-430f-b2b5-cbe02452b446.png)

#### Choose the type of access you need

Here you can choose either a single folder that is going to be created for the app or the full dropbox meaning all folders in your Dropbox.

![Dropbox Access Type](https://user-images.githubusercontent.com/56565073/169829529-e1589c2f-531d-4f63-adc2-7ee1d866676a.png)

#### Name your app

Now you can give your app a name. **_Dropbox doesn't allow you to use the word "dropbox" in your app name_**

![App Name](https://user-images.githubusercontent.com/56565073/169829431-b405acdd-0451-4a0e-96e3-610cbd27d172.png)

Once you have done all that, just click on **Create app**

#### App permissions

The app is now created, we still need to give it some permissions. Select the tab **Permissions**

![image](https://user-images.githubusercontent.com/56565073/169830023-95108a75-75cf-45b4-8d41-1729aefec576.png)

You will have a long list of different permissions. The app to work needs certain permissions, check the next ones :

* files.metadata.write
* files_metadata.read
* files_content.write
* files_content.read

The rest is not needed for the moment, however if any changes occur in the futur, you can still come back and modify the permissions.
You can now **Submit** the changes. **_If you don't see the **Submit** button, just remove the cookie popup_**

Let's head back to the **Settings** tab, we still need some informations there.

![image](https://user-images.githubusercontent.com/56565073/169831660-599f9b56-e5d1-4b4c-b4ab-39b66c0a0167.png)

First retrieve the **App key** and the **App secret** we will need them later.

![image](https://user-images.githubusercontent.com/56565073/169831930-2ae75f11-1d0c-412e-ac78-d8cabdacd319.png)

We can leave the rest for the moment, they are not important in our case.

### Docker configuration

Now you need to modify some things. Your `docker-compose.yml` and add the following service, it should look like this :

Configure every variable with your value. 

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
        # Use your AMQP credentials and channel
        AMQP_USERNAME: # root
        AMQP_PASSWORD: # root
        AMQP_HOST: # artemis
        AMQP_PORT: # 61616
        AMQP_QUEUE: # backend-event
        DROPBOX_API_KEY: # Replace this value with your API KEY
        DROPBOX_API_SECRET: # Replace this value with your API Secret
        DROPBOX_CODE: # Replace this value with the code received by Dropbox. For example: PgYD8ACqPWcAAAAAAAAATtMVR0SsNdK5hp1f-GHBl7M
        DROPBOX_CONFIG_PATH: # Choose a path for the config file like this "/app/config". DON'T FORGET THE CHANGE THE VOLUME'S NAME TOO
        API_BACKEND_URL: # Set your API Backend URL. Example : "http://api-backend"
        API_BACKEND_ID: # Set your API's Backend ID. Example : "service-account-download"
        API_CLIENT_SECRET: # Replace with your API Client's secret key. Example : "duzp0kzwDHSS2nSO46P3GBSsNnQbx8L3"
        API_TOKEN_URL: # Replace it with the keycloak's token url. Example : "http://keycloak:8080/realms/Artcoded/protocol/openid-connect/token"
        FILE_DOWNLOAD_DIR: # THE PATH MUST START WITH "/": Example "/data"
        DROPBOX_DATABASE_NAME: # Should a name for the SQLite db file. Example : "DropboxSyncDatabase"
        DROPBOX_APPDATA_PATH: # THE PATH MUST START WITH "/". Example : "/db"
        DROPBOX_CONFIG_FILE_NAME: # Choose a file name for the config file. By default it is going to be "dropbox-sync-configuration.json"
        DROPBOX_ROOT_FOLDER: # THE PATH MUST START WITH "/". Example : "/OPENARTCODED"
    volumes:
        # The mapped volumes for data et db must be the same as DROPBOX_CONFIG_PATH and FILE_DOWNLOAD_DIR and DROPBOX_APPDATA_PATH 
        - ./config/dropbox-sync:/app/config
        - ./data/dropbox-data:/data
        - ./data/dropbox-db:/db
```

## Startup

Once every precedent steps are done, you will just need to do one more thing before starting the app.

Follow the next link (don't forget to change de [API_KEY] text with your Dropbox's app API Key). 

`https://www.dropbox.com/oauth2/authorize?client_id=[API_KEY]&response_type=code&token_access_type=offline`

Retrieve the code given by Dropbox and change the `DROPBOX_CODE` environment variable in `docker-compose`.

Now you can start the service.