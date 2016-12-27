/***************************************************************************
Project:     Game Template
Copyright (c) Frills Games
Author:       Francisco Manuel Garcia Moreno (garmodev@gmail.com)
***************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class ColorExtensions{
	static System.Random _randomizer = new System.Random();

    public static Color GetMatte(this Color Source)
    {
        Color inputColor = Source;
        RGB rgb = new RGB { R = inputColor.r * 255, G = inputColor.g * 255, B = inputColor.b * 255 };
        HSB hsb = ConvertToHSB(rgb);
        hsb.B = hsb.B < 17f ? 17f : hsb.B;
        hsb.S = hsb.S>0.9f ? .9f: hsb.S; //Added to create dark on light, and light on dark
        rgb = ConvertToRGB(hsb);
        return new Color((float)rgb.R / 255, (float)rgb.G / 255, (float)rgb.B / 255, 1);
    }
    
    public static Color GetContrast(this Color Source, bool PreserveOpacity = true)
	{
		Color inputColor = Source;
		//if RGB values are close to each other by a diff less than 10%, then if RGB values are lighter side, decrease the blue by 50% (eventually it will increase in conversion below), if RBB values are on darker side, decrease yellow by about 50% (it will increase in conversion)
		float avgColorValue = ((Source.r + Source.g + Source.b) / 3);
		float diff_r = Math.Abs(Source.r - avgColorValue);
		float diff_g = Math.Abs(Source.g - avgColorValue);
		float diff_b = Math.Abs(Source.b - avgColorValue);
		if (diff_r < 20/255 && diff_g < 20/255 && diff_b < 20/255) //The color is a shade of gray
		{
			if (avgColorValue < 123/255) //color is dark
			{
				inputColor = new Color(220/255, 230/255, 50/255, Source.a);
			}
			else
			{
				inputColor = new Color(255/255, 255/255, 50/255, Source.a);
			}
		}
		float sourceAlphaValue = Source.a;
		if (!PreserveOpacity)
		{
			sourceAlphaValue = Math.Max(Source.a, 127/255); //We don't want contrast color to be more than 50% transparent ever.
		}
		RGB rgb = new RGB { R = inputColor.r*255, G = inputColor.g*255, B = inputColor.b*255 };
		HSB hsb = ConvertToHSB(rgb);
		hsb.H = hsb.H < 180 ? hsb.H + 180 : hsb.H - 180;
		//_hsb.B = _isColorDark ? 240/255 : 50/255; //Added to create dark on light, and light on dark
		rgb = ConvertToRGB(hsb);

		return new Color ((float)rgb.R/255, (float)rgb.G/255, (float)rgb.B/255, sourceAlphaValue);
	}
	
	#region Code from MSDN
	internal static RGB ConvertToRGB(HSB hsb)
	{
		// Following code is taken as it is from MSDN. See link below.
		// By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
		double chroma = hsb.S * hsb.B;
		double hue2 = hsb.H / 60;
		double x = chroma * (1 - Math.Abs(hue2 % 2 - 1));
		double r1 = 0d;
		double g1 = 0d;
		double b1 = 0d;
		if (hue2 >= 0 && hue2 < 1)
		{
			r1 = chroma;
			g1 = x;
		}
		else if (hue2 >= 1 && hue2 < 2)
		{
			r1 = x;
			g1 = chroma;
		}
		else if (hue2 >= 2 && hue2 < 3)
		{
			g1 = chroma;
			b1 = x;
		}
		else if (hue2 >= 3 && hue2 < 4)
		{
			g1 = x;
			b1 = chroma;
		}
		else if (hue2 >= 4 && hue2 < 5)
		{
			r1 = x;
			b1 = chroma;
		}
		else if (hue2 >= 5 && hue2 <= 6)
		{
			r1 = chroma;
			b1 = x;
		}
		double m = hsb.B - chroma;
		return new RGB()
		{
			R = r1 + m,
			G = g1 + m,
			B = b1 + m
		};
	}
	internal static HSB ConvertToHSB(RGB rgb)
	{
		// Following code is taken as it is from MSDN. See link below.
		// By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
		double r = rgb.R;
		double g = rgb.G;
		double b = rgb.B;
		
		double max = Max(r, g, b);
		double min = Min(r, g, b);
		double chroma = max - min;
		double hue2 = 0d;
		if (chroma != 0)
		{
			if (max == r)
			{
				hue2 = (g - b) / chroma;
			}
			else if (max == g)
			{
				hue2 = (b - r) / chroma + 2;
			}
			else
			{
				hue2 = (r - g) / chroma + 4;
			}
		}
		double hue = hue2 * 60;
		if (hue < 0)
		{
			hue += 360;
		}
		double brightness = max;
		double saturation = 0;
		if (chroma != 0)
		{
			saturation = chroma / brightness;
		}
		return new HSB()
		{
			H = hue,
			S = saturation,
			B = brightness
		};
	}
	private static double Max(double d1, double d2, double d3)
	{
		if (d1 > d2)
		{
			return Math.Max(d1, d3);
		}
		return Math.Max(d2, d3);
	}
	private static double Min(double d1, double d2, double d3)
	{
		if (d1 < d2)
		{
			return Math.Min(d1, d3);
		}
		return Math.Min(d2, d3);
	}
	internal struct RGB
	{
		internal double R;
		internal double G;
		internal double B;
	}
	internal struct HSB
	{
		internal double H;
		internal double S;
		internal double B;
	}
	#endregion //Code from MSDN
}
