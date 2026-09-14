using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;

using static System.Console;

namespace Kiogas;

public class Parser
{
    private bool inArr = false; // currently inside of an array
    private bool inObj = false; // currently inside of an object

    private Dictionary<string, Data> data = new();
    private List<string> names = new();

    private bool IsDuplicate(string name)
    {
        if (names.Contains(name))
        {
            Console.WriteLine($"name.duplicate: There are two or more keys named {name}");
            return true;
        }
        return false;
    }

    private bool ValidateType(string type, string val, uint ln)
    {
        if (val == null)
        {
            return true;
        }

        return type switch
        {
            "byte" => IsIt.U8(val, ln),
            "int" => IsIt.Int(val, ln),
            "uint" => IsIt.Positive(val, ln),
            "flt" => IsIt.Flt(val, ln),
            "str" => IsIt.Str(val, ln),
            "bool" => Helper.Bools.Contains(val),
            _ => true
        };
    }

    public Dictionary<string, Data> Parse(string fn)
    {
        string[] lines = File.ReadAllLines(fn);
        for (uint i = 0; i < lines.Length; i++)
        {
            int __i = Convert.ToInt32(i);
            uint lineNum = i + 1;
            string line = lines[__i].Trim();

            if (string.IsNullOrWhiteSpace(line) || line[0] == '|')
            {
                continue;
            }

            if (line.StartsWith("->"))
            {
                inArr = false;
                if (line.Contains("->") && line != "->")
                {
                    WriteLine($"arr.terminator.polluted [{lineNum}]: Polluted array terminator (the end of an array should be JUST '->', NOTHING else)");
                    hasERRORS = true;
                    errorCOUNT++;
                }
                continue;
            }
            if (line.StartsWith("=>>"))
            {
                inObj = false;
                if (line.Contains("=>>") && line != "=>>")
                {
                    WriteLine($"obj.terminator.polluted [{lineNum}]: Polluted object terminator (the end of an object should be JUST '=>>', NOTHING else)");
                    hasERRORS = true;
                    errorCOUNT++;
                }
            }
            string type = Helper.GetType(line, lineNum);
            if (string.IsNullOrEmpty(type))
            {
                continue;
            }

            string[] _parts = line.Split(':', 2);
            _parts[0] = _parts[0].Trim();

            // debug
            // WriteLine($"debug | type: {type}");

            if (_parts.Length < 2 && (!type.StartsWith("arr.") && type != "obj"))
            {
                WriteLine($"key.value.missing [{lineNum}]: Key {_parts[0]} was not given a value.");
                hasERRORS = true;
                errorCOUNT++;
            }

            string name = _parts[0].Split(' ')[1];

            if (name.StartsWith("<-") && type.StartsWith("arr."))
            {
                name = name[2..];
            }

            if (IsDuplicate(name))
            {
                hasERRORS = true;
                errorCOUNT++;
            }

            names.Add(name);

            string val = "";

            if (_parts.Length > 1)
            {
                val = _parts[1].Trim();
                if (val == "empty")
                {
                    val = null;
                }
            }
            if (type == "str" && val != null && val.Contains('\\'))
            {
                val = Helper.Unquote(val, lineNum);
                val = Helper.EscapeCheck(val, lineNum);
            }

            bool isArrayType = type.StartsWith("arr.");
            bool isObjectType = (type == "obj");
            if (type == "bool")
            {
                if (Helper.truthy.Contains(val)) val = "true";
                else if (Helper.falsy.Contains(val)) val = "false";
                else
                {
                    hasERRORS = true;
                    errorCOUNT++;
                    WriteLine($"bool.invalid [{lineNum}]: {val} is not a valid boolean.");
                }
            }
            // tuxzilla wuz here, this makes it so types are always.. the types they should be
            if (!isArrayType && !isObjectType)
            {
                if (!ValidateType(type, val, lineNum))
                {
                    hasERRORS = true;
                    errorCOUNT++;
                }
            }

            data[name] = new Data
            {
                Name = name,
                Value = val,
                Type = type,
                Array = new List<object>(),
                IsArr = isArrayType,
                Object = new Dictionary<string, object>(),
                IsObj = isObjectType,
                ObjType = null
            };

            if (isArrayType)
            {
                inArr = true;
                i++;

                while (inArr && i < lines.Length)
                {
                    string arrLine = lines[(int)i].Trim();
                    uint arrLineNum = i + 1;

                    if (arrLine == "->")
                    {
                        inArr = false;
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(arrLine) || arrLine[0] == '|')
                    {
                        i++;
                        continue;
                    }

                    switch (data[name].Type)
                    {
                        case "arr.u32":
                            if (IsIt.Positive(arrLine, arrLineNum))
                            {
                                data[name].Array.Add(Convert.ToUInt32(arrLine));
                            }
                            break;
                        case "arr.int":
                            if (IsIt.Int(arrLine, arrLineNum))
                            {
                                data[name].Array.Add(Convert.ToInt32(arrLine));
                            }
                            break;
                        case "arr.str":
                            if (IsIt.Str(arrLine, arrLineNum))
                            {
                                data[name].Array.Add(Helper.Unquote(arrLine, arrLineNum));
                            }
                            break;
                        case "arr.bool":
                            if (Helper.Bools.Contains(arrLine))
                            {
                                data[name].Array.Add(Helper.Boolify(arrLine));
                            }
                            break;
                        case "arr.flt":
                            if (IsIt.Flt(arrLine, arrLineNum))
                            {
                                data[name].Array.Add(Convert.ToDouble(arrLine));
                            }
                            break;
                        case "arr.u8":
                            if (IsIt.U8(arrLine, arrLineNum))
                            {
                                data[name].Array.Add(Convert.ToByte(arrLine));
                            }
                            break;
                        case "arr.u64":
                            if (IsIt.u64(arrLine, arrLineNum)) data[name].Array.Add(Convert.ToUInt64(arrLine));
                            break;
                        case "arr.i64":
                            if (IsIt.i64(arrLine, arrLineNum)) data[name].Array.Add(Convert.ToInt64(arrLine));
                            break;
                        case "arr.i16":
                            if (IsIt.i16(arrLine, arrLineNum)) data[name].Array.Add(Convert.ToInt16(arrLine));
                            break;
                        case "arr.u16":
                            if (IsIt.u16(arrLine, arrLineNum)) data[name].Array.Add(Convert.ToUInt16(arrLine));
                            break;
                        default:
                            break;
                    }
                    i++;
                }
            }

            if (isObjectType)
            {
                inObj = true;
                int keyNum = 0;
                i++;

                if (data[name].Object == null)
                {
                    data[name].Object = new Dictionary<string, object>();
                }
                if (data[name].ObjType == null)
                {
                    data[name].ObjType = new List<string>();
                }

                while (inObj && i < lines.Length)
                {
                    string objLine = lines[(int)i].Trim();
                    uint objln = i + 1;

                    if (objLine == "==>")
                    {
                        inObj = false;
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(objLine) || objLine[0] == '|')
                    {
                        i++;
                        continue;
                    }

                    if (!objLine.Contains(':'))
                    {
                        WriteLine($"obj.missingColon [{objLine}]: Missing colon.");
                        i++;
                        hasERRORS = true;
                        errorCOUNT++;
                    }

                    string[] parts = objLine.Split(':', 2);
                    string key = parts[0].Trim();
                    string keyVal = parts[1].Trim();

                    data[name].Object[key] = keyVal;

                    // infer key type
                    data[name].ObjType.Add(Helper.GetType(objLine, objln));
                    string t = data[name].ObjType[keyNum];

                    if (t == "str" && keyVal.Contains('\\'))
                    {
                        keyVal = Helper.Unquote(keyVal, objln);
                        keyVal = Helper.EscapeCheck(keyVal, objln);
                        // the Helper.escapeCheck returns .:ERR:.
                        // when something goes wrong
                        // so this just breaks out if
                        // it sees that value
                        // - wer
                        if (keyVal == ".:ERR:.") break;

                        data[name].Object[key] = keyVal;
                    }

                    if (t == "obj")
                    {
                        WriteLine($"obj.nested [{objln}]: Nested objects are not supported.");
                        i++;
                        hasERRORS = true;
                        errorCOUNT++;
                    }

                    keyNum++;
                    i++;
                }

                // debug
                int j = 0; // what does this even do??? - Centurion
                           // it's a debug thing to print out
                           // object key/value pairs - wer

                foreach (var x in data[name].Object)
                {
                    WriteLine("DEBUG:");
                    WriteLine($"{x.Key}: [{data[name].ObjType[j]}] = {x.Value}");
                    j++;
                }
            }
        }

        foreach (var kvp in data)
        {
            if (kvp.Value.IsArr)
            {
                WriteLine($"{kvp.Key}: [{kvp.Value.Type}] = {string.Join(", ", kvp.Value.Array)}");
            }
            else
            {
                WriteLine($"{kvp.Key}: [{kvp.Value.Type}] = {kvp.Value.Value}");
            }
        }
        if (!hasERRORS) WriteLine("Parse successful!");
        else WriteLine($"Parse finished with {errorCOUNT} errors.");
        return data;
    }
}
