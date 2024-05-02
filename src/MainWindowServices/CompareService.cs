﻿using System;
using System.Windows;
using System.Windows.Controls;
using CudaHelioCommanderLight.Enums;

namespace CudaHelioCommanderLight.MainWindowServices;

public class CompareService
{
    public (string?, LibStructureType) CompareWithLib(string tag, ComboBox geliosphereLibRatio, ComboBox geliosphereLibType)
    {
        LibStructureType libStructureType = LibStructureType.DIRECTORY_SEPARATED;
        string libPath = string.Empty;
        switch (tag)
        {
            case "libHelium":
            {
                libPath = @"libFiles\lib-helium";
                break;
            }
            case "libProton":
            {
                libPath = @"libFiles\lib-proton";
                break;
            }
            case "forceField":
            {
                libPath = @"libFiles\lib-forcefield2023";
                libStructureType = LibStructureType.FILES_FORCEFIELD2023_COMPARISION;
                break;
            }
            case "geliosphere":
            {
                if (geliosphereLibRatio.SelectedItem == null)
                {
                    MessageBox.Show("Library not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return (null, libStructureType);
                }

                var libtype = geliosphereLibType.SelectedItem.ToString();
                var libRatio = geliosphereLibRatio.SelectedItem.ToString().Replace(',', '.');
                libPath = $"libFiles\\lib-geliosphere-{libtype}-{libRatio}";
                libStructureType = LibStructureType.FILES_SOLARPROP_LIB;
                break;
            }
        }

        return (libPath, libStructureType);
    }
}