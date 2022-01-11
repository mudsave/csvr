using System;
using System.Collections.Generic;
using LitJson;

public class CGearEditor : CEntityEditor
{
    public int relevanceID = 0;

    public override JsonData WriteJson()
    {
        JsonData datas = base.WriteJson();

        datas["relevanceID"] = relevanceID;

        return datas;
    }
}

