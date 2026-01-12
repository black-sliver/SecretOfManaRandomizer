#pragma once

#include <stdint.h>
// #include <uchar.h> // not available on macos
typedef uint16_t char16_t;


typedef struct SoMR_ItemList SoMR_ItemList;
typedef struct SoMR_Item SoMR_Item;
typedef struct SoMR_LocationList SoMR_LocationList;
typedef struct SoMR_Location SoMR_Location;
typedef struct SoMR_OWSettings SoMR_OWSettings;
typedef struct SoMR_OWGenerator SoMR_OWGenerator;
typedef struct SoMR_Context SoMR_Context;
typedef struct SoMR_StrList SoMR_StrList;
typedef struct SoMR_WorkingData SoMR_WorkingData;

SoMR_ItemList* SoMR_OW_GetAllItems();
SoMR_LocationList* SoMR_OW_GetAllLocations();
SoMR_OWSettings* SoMR_OW_NewSettings();
SoMR_OWGenerator* SoMR_OW_NewGenerator();
SoMR_Context* SoMR_OW_Init(char16_t* src, char16_t* seed, SoMR_OWGenerator* generator, SoMR_OWSettings* settings);
void SoMR_OW_Run(char16_t* dst, char16_t* seed, SoMR_OWGenerator* generator, SoMR_OWSettings* settings, SoMR_Context* context);

int32_t SoMR_ItemList_Count(SoMR_ItemList* list);
SoMR_Item* SoMR_ItemList_At(SoMR_ItemList* list, int32_t index);
void SoMR_ItemList_Unref(SoMR_ItemList* list);
char16_t* SoMR_Item_GetName(SoMR_Item* item);
char16_t* SoMR_Item_GetType(SoMR_Item* item);
uint8_t SoMR_Item_GetEventFlag(SoMR_Item* item);
uint8_t SoMR_Item_GetItemId(SoMR_Item* item);
void SoMR_Item_Unref(SoMR_Item* item);

int32_t SoMR_LocationList_Count(SoMR_LocationList* list);
SoMR_Location* SoMR_LocationList_At(SoMR_LocationList* list, int32_t index);
void SoMR_LocationList_Unref(SoMR_LocationList* list);
char16_t* SoMR_Location_GetName(SoMR_Location* location);
int32_t SoMR_Location_GetMapNum(SoMR_Location* location);
int32_t SoMR_Location_GetObjNum(SoMR_Location* location);
int32_t SoMR_Location_GetEventNum(SoMR_Location* location);
int32_t SoMR_Location_GetEventIndex(SoMR_Location* location);
uint8_t SoMR_Location_GetLocationId(SoMR_Location* location);
void SoMR_Location_Unref(SoMR_Location* location);
SoMR_StrList* SoMR_Location_GetRequirements(SoMR_Location* location);

SoMR_ItemList* SoMR_OWGenerator_GetItems(SoMR_OWGenerator* generator);
SoMR_LocationList* SoMR_OWGenerator_GetLocations(SoMR_OWGenerator* generator);
void SoMR_OWGenerator_Unref(SoMR_OWGenerator* generator);

void SoMR_OWSettings_SetStr(SoMR_OWSettings* settings, char16_t* key, char16_t* value);
char16_t* SoMR_OWSettings_Dump(SoMR_OWSettings* settings);
void SoMR_OWSettings_Unref(SoMR_OWSettings* settings);

char16_t* SoMR_Context_GetError(SoMR_Context* context);
SoMR_WorkingData* SoMR_Context_GetWorkingData(SoMR_Context* context);
void SoMR_Context_Unref(SoMR_Context* context);

char16_t* SoMR_WorkingData_GetStr(SoMR_WorkingData* data, char16_t* key);
void SoMR_WorkingData_SetStr(SoMR_WorkingData* data, char16_t* key, char16_t* value);
char16_t* SoMR_WorkingData_Dump(SoMR_WorkingData* data);
void SoMR_WorkingData_Unref(SoMR_WorkingData* data);

int32_t SoMR_StrList_Count(SoMR_StrList* lst);
char16_t* SoMR_StrList_At(SoMR_StrList* lst, int32_t index);
void SoMR_StrList_Unref(SoMR_StrList* list);

void SoMR_Str_Free(char16_t* string);
