# uMod Remote Whitelist plugin
Whitelist is loaded from file `.../oxide/data/{WhitelistFileName}.json`.
This plugin use [CanUserLogin hook](https://umod.org/documentation/api/hooks). If player (playerId e.g. steamId) is on the list (field `members`) can connect to server.
File can be updated manually or automatically by http server.

[Free http server](https://rust.choodiesam.com/) for creating and hosting whitelist file. Just go to [dashboard](https://rust.choodiesam.com/dashboard) and sign in with Steam account.

### Config
Configuration parameters are define in file `.../oxide/config/RemoteWhitelist.json`.

When loading the plugin, the file will be created with default values if it does not exist.
#### Options
- ApiTimeout: http request timeout in seconds
- ApiToken: The token is added at the end of the `ApiUrl` when sent request e.g. `https://rust.choodiesam.com/api/rust-plugin/whitelist/7b7730fd8ae6d9d0b7a283c2b95650e021213849ddb7685ed99a5437421f69f2`
- ApiUrl: Whitelist endpoint e.g. `https://rust.choodiesam.com/api/rust-plugin/whitelist/`
- UpdateInterval: How often a request to update the list is sent. If set value less than 10 seconds remote update is disabled and you must update file manually.
- WhitelistFileName: File name of whitelist located at `.../oxide/data/{WhitelistFileName}.json` e.g. `/server/oxide/data/WhitelistSteamIds.json`
#### Examples
##### Cofig file (default values)
```
{
  "ApiTimeout": 2,
  "ApiToken": "7b7730fd8ae6d9d0b7a283c2b95650e021213849ddb7685ed99a5437421f69f2",
  "ApiUrl": "https://rust.choodiesam.com/api/rust-plugin/whitelist/",
  "UpdateInterval": 60,
  "WhitelistFileName": "WhitelistSteamIds"
}
```
##### Whitelist file or http response from api endpoint
```
{
  "members": ["96864198385386831"],
  "createdAt":"2023-08-20T10:54:19.936Z"
}
```
