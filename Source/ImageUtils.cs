using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public static class ImageUtils {

	private static Bitmap m_bmpScreenCache = null;

	public static Bitmap TakeScreenShot(int screenLeft, int screenTop, int width, int height) {
		//bmps have to be Disposed or we run out of GDI resoureces, so we try and cache the previous one if possible.
		if(m_bmpScreenCache == null)
			m_bmpScreenCache = new Bitmap(width, height);
		else if(m_bmpScreenCache.Width != width || m_bmpScreenCache.Height != height)
		{
			m_bmpScreenCache.Dispose();
			m_bmpScreenCache = new Bitmap(width, height);
		}

		using(Graphics g = Graphics.FromImage(m_bmpScreenCache))
		{
			g.CopyFromScreen(screenLeft, screenTop, 0, 0, m_bmpScreenCache.Size);
		}

		// remember, this Bitmap needs to be Dispose()'d!
		return m_bmpScreenCache;
	}

	public static Bitmap TakeScreenShot () {
        int screenLeft   = SystemInformation.VirtualScreen.Left;
        int screenTop    = SystemInformation.VirtualScreen.Top;
        int screenWidth  = SystemInformation.VirtualScreen.Width;
        int screenHeight = SystemInformation.VirtualScreen.Height;

		//bmps have to be Disposed or we run out of GDI resoureces, so we try and cache the previous one if possible.
		if(m_bmpScreenCache == null)
			m_bmpScreenCache = new Bitmap(screenWidth, screenHeight);
		else if(m_bmpScreenCache.Width != screenWidth || m_bmpScreenCache.Height != screenHeight) {
			m_bmpScreenCache.Dispose();
			m_bmpScreenCache = new Bitmap(screenWidth, screenHeight);
		}

        using (Graphics g = Graphics.FromImage(m_bmpScreenCache)) {
            g.CopyFromScreen(screenLeft, screenTop, 0, 0, m_bmpScreenCache.Size);
        }

        return m_bmpScreenCache;
    }

    public static Rectangle SearchBitmap (Bitmap smallBmp, Bitmap bigBmp, double tolerance) {
        BitmapData smallData = smallBmp.LockBits(new Rectangle(0, 0, smallBmp.Width, smallBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        BitmapData bigData = bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        int smallStride = smallData.Stride;
        int bigStride = bigData.Stride;

        int bigWidth = bigBmp.Width;
        int bigHeight = bigBmp.Height - smallBmp.Height + 1;
        int smallWidth = smallBmp.Width * 3;
        int smallHeight = smallBmp.Height;

        Rectangle location = Rectangle.Empty;
        int margin = Convert.ToInt32(255.0 * tolerance);

        unsafe
        {
            byte* pSmall = (byte*)(void*)smallData.Scan0;
            byte* pBig = (byte*)(void*)bigData.Scan0;

            int smallOffset = smallStride - smallBmp.Width * 3;
            int bigOffset = bigStride - bigBmp.Width * 3;

            bool matchFound = true;

            for (int y = 0; y < bigHeight; y++) {
                for (int x = 0; x < bigWidth; x++) {
                    byte* pBigBackup = pBig;
                    byte* pSmallBackup = pSmall;

                    //Look for the small picture.
                    for (int i = 0; i < smallHeight; i++) {
                        int j = 0;
                        matchFound = true;
                        for (j = 0; j < smallWidth; j++) {
                            //With tolerance: pSmall value should be between margins.
                            int inf = pBig[0] - margin;
                            int sup = pBig[0] + margin;
                            if (sup < pSmall[0] || inf > pSmall[0]) {
                                matchFound = false;
                                break;
                            }

                            pBig++;
                            pSmall++;
                        }

                        if (!matchFound)
                            break;

                        //We restore the pointers.
                        pSmall = pSmallBackup;
                        pBig = pBigBackup;

                        //Next rows of the small and big pictures.
                        pSmall += smallStride * (1 + i);
                        pBig += bigStride * (1 + i);
                    }

                    //If match found, we return.
                    if (matchFound) {
                        location.X = x;
                        location.Y = y;
                        location.Width = smallBmp.Width;
                        location.Height = smallBmp.Height;
                        break;
                    }
                    //If no match found, we restore the pointers and continue.
                    else {
                        pBig = pBigBackup;
                        pSmall = pSmallBackup;
                        pBig += 3;
                    }
                }

                if (matchFound)
                    break;

                pBig += bigOffset;
            }
        }

        bigBmp.UnlockBits(bigData);
        smallBmp.UnlockBits(smallData);

        return location;
    }

    public static ulong AverageBrightness (Bitmap bmp) {
        BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        
        int stride = data.Stride;

        int width = bmp.Width;
        int height = bmp.Height;

        ulong brightness = 0;

        unsafe
        {
            byte* p = (byte*)(void*)data.Scan0;

            for (int column = 0; column < height; column++) {
                for (int row = 0; row < width; row++) {
                    int index = column * stride + row * 3;
                    brightness += p[index];
                    brightness += p[index + 1];
                    brightness += p[index + 2];
                }
            }
        }

        bmp.UnlockBits(data);

        return brightness / (ulong)width / (ulong)height;
    }
}
