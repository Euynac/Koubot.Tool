using System.Collections.Generic;

namespace Koubot.Tool.General.UnitConvert;

public class InformationUnitConverter : UnitConverterBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="useSI">国际单位制（法语：Système International d'Unités 符号：SI）</param>
    public InformationUnitConverter(bool useSI = false) : base(() =>
    {
        var factor = useSI? 1000: 1024;
        var bit = new Unit("bit", 1, "比特");
        var @byte = new Unit("bytes", 8, bit, "字节");
        var kb = new Unit("kB", factor, @byte, "KiB|K|千字节");
        var mb = new Unit("MB", factor, kb, "MiB|M|兆|MegaByte");
        var gb = new Unit("GB", factor, mb, "GiB|G|GigaByte");
        var tb = new Unit("TB", factor, gb, "TiB|T|TeraByte");
        var pb = new Unit("PB", factor, tb, "PiB|P|PetaByte");
        var eb = new Unit("EB", factor, pb, "EiB|ExaByte");
        var zb = new Unit("ZB", factor, eb, "ZiB|ZettaByte");
        var yb = new Unit("YB", factor, zb, "YiB|YottaByte");
        return new List<Unit> {bit, @byte, kb, mb, gb, tb, pb, eb, zb, yb};
    })
    {
        
    }

}