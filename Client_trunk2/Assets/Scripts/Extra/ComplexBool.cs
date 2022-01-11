using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//复合BOOL型
public class ComplexBool
{
    protected int intComplex = 0;
    protected Dictionary<string, bool> strComplex = new Dictionary<string, bool>();
    protected bool refvalue = true;

    public ComplexBool(bool _refvalue)
    {
        refvalue = _refvalue;
    }

    public ComplexBool(bool _disrefvalue, int complex)
    {
        refvalue = !_disrefvalue;
        intComplex = complex;
    }

    public bool Value()
    {
        if (intComplex == 0 && strComplex.Count == 0)
            return refvalue;

        return !refvalue;
    }

    public void addComplex()
    {
        intComplex++;
    }

    public void subComplex()
    {
        if (intComplex <= 0)
            return;
        
        intComplex--;
    }

    public void addComplex(string name)
    {
        strComplex[name] = true;
    }

    public void subComplex(string name)
    {
        strComplex.Remove(name);
    }
}
