# GCPlayFabCLI

Simple CLI Tool to help migrating data from one PlayFab title to another.

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
cp catalogv2 <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -xv -v
```

Params:
* sourceTitleId: Id of the title in which you want to copy the catalog *FROM*
* sourceDevSecretKey: Secret Key of the title in which you want to copy the catalog *FROM*
* targetTitleId: Id of the title in which you want to copy the catalog *TO*
* targetDevSecretKey: Secret key of the title in which you want to copy the catalog *TO*
* -xv: Optional, delete everything from target and create new (default won't delete if item ID from target matches one to be created from the source; don't update price options though)
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

## Server-wide Title Data Usage

### Listing Title Data and Internal Data

```
ls titledata <titleId> <devSecretKey>
```

```
ls titleinternaldata <titleId> <devSecretKey>
```

Params:
* titleId: Id of the title
* devSecretKey: Secret key of the title

### Migrating Title Data and Internal Data

This will copy title data and internal data from the provided source title to the target title. 
Notice that it won't copy the build alias, so you still have to do it yourself once you set up servers.

```
cp titledata <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -v
```

```
cp titleinternaldata <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -v
```

Params:
* sourceTitleId: Id of the title in which you want to copy the catalog *FROM*
* sourceDevSecretKey: Secret Key of the title in which you want to copy the catalog *FROM*
* targetTitleId: Id of the title in which you want to copy the catalog *TO*
* targetDevSecretKey: Secret key of the title in which you want to copy the catalog *TO*
* -v: verbose (optional) 

## Matchmaking Queues Usage

### Listing Matchmaking Queues

```
ls matchmakingqueues <titleId> <devSecretKey>
```

Params:
* titleId: Id of the title
* devSecretKey: Secret key of the title

### Migrating Matchmaking Queues

This will copy the matchmaking queues configs from the provided source title to the target title.

```
cp matchmakingqueues <sourceTitleId> <sourceDevSecretKey> <targetTitleId> <targetDevSecretKey> -v
```

Params:
* sourceTitleId: Id of the title in which you want to copy the catalog *FROM*
* sourceDevSecretKey: Secret Key of the title in which you want to copy the catalog *FROM*
* targetTitleId: Id of the title in which you want to copy the catalog *TO*
* targetDevSecretKey: Secret key of the title in which you want to copy the catalog *TO*
* -v: verbose (optional) 

---
# Exporting Data

## Exporting Players Feedbacks to CSV

```
export player feedback <segmentId> <titleId> <devSecretKey> <fileName.csv> -v
```

Params:
* segmentId: ID of the player segment to be exported (keep in mind that each title has a different ID)
* titleId: Id of the title 
* devSecretKey: Secret Key of the title 
* fileName.csv: name of the exported file
* -v: verbose (optional) 

---
# Deleting

## Deleting Player's Inventory Items

```
delete player inventory <titleId> <devSecretKey> <titlePlayerAccountId> <collectionId> <friendly.item.id1> <friendly.item.id2> <friendly.item.idN> -v
```

Params:
* titleId: Id of the title 
* devSecretKey: Secret Key of the title
* titlePlayerAccountId: ID of the player in that title
* collectionId: inventory collection id to delete from
* friendly.item.id1: Friendly item ID (the AlteranteId in PlayFab) (optional, if not specified all inventory items will be deleted, one by one)
* -v: verbose (optional) 

*NOTE: If not items is specified, all inventory items will be deleted!*

---
# Adding

## Adding Player's Inventory Items

```
add player inventory <titleId> <devSecretKey> <titlePlayerAccountId> <collectionId> <friendly.item.id1> <friendly.item.id2> <friendly.item.idN> -v
```

Params:
* titleId: Id of the title 
* devSecretKey: Secret Key of the title
* titlePlayerAccountId: ID of the player in that title
* collectionId: inventory collection id to delete from
* friendly.item.id1: Friendly item ID (the AlteranteId in PlayFab)
* -v: verbose (optional) 

*NOTE: This is going to add one single item even if the list has repeated items (Amount = 1). Not meant to be used with currencies or stackable items. You can use with stackble items, but you're going to get one single item at a time (not efficient if you want to add lots of the same item).*
