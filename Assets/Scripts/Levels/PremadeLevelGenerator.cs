using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PremadeLevelGenerator
{
    public static void GenerateFirstLevel(Level level)
    {
        var asset = Resources.Load<TextAsset>("first-level");

        if (asset == null)
        {
            throw new FileNotFoundException("Cannot find the file.", "first-level.txt");
        }

        level.Size = 12;
        level.Map = new CellType[level.Size, level.Size];
        level.Objects = new ILevelObject[level.Size, level.Size];
        level.Units = new Unit[level.Size, level.Size];

        int i = 0;
        using (StringReader sr = new StringReader(asset.text))
        {
            while (true)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    foreach (var x in line)
                    {
                        if (i == 144)
                        {
                            break;
                        }
                        level.Map[i % level.Size, i / level.Size] = (CellType)x;
                        i++;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    public static void GenerateBossLevel(Level level)
    {
        var asset = Resources.Load<TextAsset>("boss-level");

        if (asset == null)
        {
            throw new FileNotFoundException("Cannot find the file.", "boss-level.txt");
        }

        level.Size = 12;
        level.Map = new CellType[level.Size, level.Size];
        level.Objects = new ILevelObject[level.Size, level.Size];
        level.Units = new Unit[level.Size, level.Size];

        int i = 0;
        using (StringReader sr = new StringReader(asset.text))
        {
            while (true)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    foreach (var x in line)
                    {
                        if (i == 144)
                        {
                            break;
                        }
                        level.Map[i % level.Size, i / level.Size] = (CellType)x;
                        i++;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
