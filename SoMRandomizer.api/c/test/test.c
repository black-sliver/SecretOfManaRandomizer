#include <assert.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <uchar.h>

#include "../api.h"


void printString(char16_t* ws)
{
    _Static_assert(sizeof(*ws) == 2);
    // we assume this is all ascii
    for (size_t i = 0; i < ws[i]; i++)
    {
        uint16_t wc = ws[i];
        if (wc & 0xff)
            printf("%c", (char)(wc&0xff));
    }
}

int main(int argc, char** argv)
{
    SoMR_ItemList* items = SoMR_OW_GetAllItems();
    printf("%d ITEMS:\n", SoMR_ItemList_Count(items));
    int i = 0;
    while (true)
    {
        SoMR_Item* item = SoMR_ItemList_At(items, i++);
        if (!item)
            break;
        char16_t* name = SoMR_Item_GetName(item);
        printString(name);
        printf("\n");
        SoMR_Str_Free(name);
        SoMR_Item_Unref(item);
    }
    SoMR_ItemList_Unref(items);

    printf("\n---\n\n");

    SoMR_LocationList* locations = SoMR_OW_GetAllLocations();
    printf("%d LOCATIONS:\n", SoMR_LocationList_Count(locations));
    i = 0;
    while (true)
    {
        SoMR_Location* location = SoMR_LocationList_At(locations, i++);
        if (!location)
            break;
        char16_t* name = SoMR_Location_GetName(location);
        printString(name);
        printf("\n");
        SoMR_Str_Free(name);
        SoMR_Location_Unref(location);
    }
    SoMR_LocationList_Unref(locations);

    printf("\n---\n\nDone.\n");
}
