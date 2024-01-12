# GCPlayFabCLI

## Catalog V2 Usage

### Listing Catalog V2

```
ls catalogv2 <titleId> <devSecretKey>
```

Params:
* titleId: Id of the title
* devSecretKey: Secret key of the title

### Migrating Catalog V2

This will copy catalog items, currency, bundles and stores from the provided source title to the target title.

```
cp catalogv2 <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -v
```

Params:
* sourceTitleId: Id of the title in which you want to copy the catalog *FROM*
* sourceDevSecretKey: Secret Key of the title in which you want to copy the catalog *FROM*
* targetTitleId: Id of the title in which you want to copy the catalog *TO*
* targetDevSecretKey: Secret key of the title in which you want to copy the catalog *TO*
* -v: verbose (optional) 

## CloudScript Usage

### Listing Existing Functions

```
ls functions <titleId> <devSecretKey>
```

Params:
* titleId: Id of the title
* devSecretKey: Secret key of the title

### Migrating CloudScript Functions

This will copy the all the cloudscript functions from the provided source title to the target title.

```
cp functions <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -v
```

Params:
* sourceTitleId: Id of the title in which you want to copy the catalog *FROM*
* sourceDevSecretKey: Secret key of the title in which you want to copy the catalog *FROM*
* targetTitleId: Id of the title in which you want to copy the catalog *TO*
* targetDevSecretKey: Secret key of the title in which you want to copy the catalog *TO*
* -v: verbose (optional) 
