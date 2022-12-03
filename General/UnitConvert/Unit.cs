using System.Collections.Generic;
using System.Linq;

namespace Koubot.Tool.General.UnitConvert;

public class UnitValue
{
    public Unit Unit { get; set; }
    public double Value { get; set; }

    public UnitValue(double value, Unit unit)
    {
        Value = value;
        Unit = unit;
    }
    public UnitValue ConvertToUnit(Unit toUnit)
    {
        return new UnitValue(toUnit.FromBaseValue(Unit.ToBaseValue(Value)), toUnit);
    }

    public override string ToString()
    {
        return $"{Value}{Unit.Symbol}";
    }
}
public class Unit
{
    public string Symbol { get; set; }
    public List<string> UnitNames { get; set; } 
    /// <summary>
    /// Factor to base.
    /// </summary>
    public double Factor { get; set; }
    /// <summary>
    /// Introduction of this unit.
    /// </summary>
    public string? Description { get; set; }

    public override string ToString() => Symbol;

    public Unit(string symbol, double factor, string? alias = null)
    {
        Symbol = symbol;
        Factor = factor;
        UnitNames = new List<string> {Symbol};
        UnitNames.AddRange(alias?.Split('|').ToList() ?? new List<string>());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <param name="factor">one base unit value equal to how many current unit?</param>
    /// <param name="symbol"></param>
    /// <param name="alias"></param>
    public Unit(Unit baseUnit, double factor, string symbol, string? alias = null) :
        this(symbol, baseUnit.Factor / factor, alias)
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="factor">one current unit value equal to how many base unit?</param>
    /// <param name="baseUnit"></param>
    /// <param name="alias"></param>
    public Unit(string symbol, double factor, Unit baseUnit, string? alias = null) :
        this(symbol, baseUnit.Factor * factor, alias)
    {

    }
    /// <summary>
    /// From factor 1 base unit value to current unit value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public double FromBaseValue(double value) => value / Factor;
    /// <summary>
    /// From current unit value to factor 1 base unit value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public double ToBaseValue(double value) => value * Factor;

    public UnitValue GetUnitValue(double currentUnitValue) => new(currentUnitValue, this);
    public UnitValue ConvertToUnit(double currentUnitValue, Unit toUnit)
    {
        return GetUnitValue(currentUnitValue).ConvertToUnit(toUnit);
    } 
}