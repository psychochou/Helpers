using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;

namespace Shared
{ /// <summary>
  /// Contains helpers for dealing with byte-hex char conversions.
  /// </summary>
    internal static class HexUtil
    {
        /// <summary>
        /// Converts a number 0 - 15 to its associated hex character '0' - 'F'.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static char UInt32LsbToHexDigit(uint value)
        {
            Debug.Assert(value < 16);
            return (value < 10) ? (char)('0' + value) : (char)('A' + (value - 10));
        }

        /// <summary>
        /// Converts a number 0 - 15 to its associated hex character '0' - 'F'.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static char Int32LsbToHexDigit(int value)
        {
            Debug.Assert(value < 16);
            return (char)((value < 10) ? ('0' + value) : ('A' + (value - 10)));
        }

        /// <summary>
        /// Gets the uppercase hex-encoded form of a byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ByteToHexDigits(byte value, out char firstHexChar, out char secondHexChar)
        {
            firstHexChar = UInt32LsbToHexDigit((uint)value >> 4);
            secondHexChar = UInt32LsbToHexDigit((uint)value & 0xFU);
        }
    }
    // These sources are taken from corclr repo (src\mscorlib\src\System\Buffer.cs with x64 path removed)
    // The reason for this duplication is that System.Runtime.dll 4.0.10 did not expose Buffer.MemoryCopy,
    // but we need to make this component work with System.Runtime.dll 4.0.10
    // The methods AreOverlapping and SlowCopyBackwards are not from Buffer.cs. Buffer.cs does an internal CLR call for these.
    static class BufferInternal
    {
        // This method has different signature for x64 and other platforms and is done for performance reasons.
        [System.Security.SecurityCritical]
        private static unsafe void Memmove(byte* dest, byte* src, uint len)
        {
            if (AreOverlapping(dest, src, len))
            {
                SlowCopyBackwards(dest, src, len);
                return;
            }

            // This is portable version of memcpy. It mirrors what the hand optimized assembly versions of memcpy typically do.
            switch (len)
            {
                case 0:
                    return;
                case 1:
                    *dest = *src;
                    return;
                case 2:
                    *(short*)dest = *(short*)src;
                    return;
                case 3:
                    *(short*)dest = *(short*)src;
                    *(dest + 2) = *(src + 2);
                    return;
                case 4:
                    *(int*)dest = *(int*)src;
                    return;
                case 5:
                    *(int*)dest = *(int*)src;
                    *(dest + 4) = *(src + 4);
                    return;
                case 6:
                    *(int*)dest = *(int*)src;
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    return;
                case 7:
                    *(int*)dest = *(int*)src;
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    *(dest + 6) = *(src + 6);
                    return;
                case 8:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    return;
                case 9:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(dest + 8) = *(src + 8);
                    return;
                case 10:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    return;
                case 11:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    *(dest + 10) = *(src + 10);
                    return;
                case 12:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    return;
                case 13:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(dest + 12) = *(src + 12);
                    return;
                case 14:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    return;
                case 15:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    *(dest + 14) = *(src + 14);
                    return;
                case 16:
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(int*)(dest + 12) = *(int*)(src + 12);
                    return;
                default:
                    break;
            }

            if ((unchecked((int)dest) & 3) != 0)
            {
                if (((int)dest & 1) != 0)
                {
                    *dest = *src;
                    src++;
                    dest++;
                    len--;
                    if (((int)dest & 2) == 0)
                        goto Aligned;
                }
                *(short*)dest = *(short*)src;
                src += 2;
                dest += 2;
                len -= 2;
                Aligned:;
            }

            uint count = len / 16;
            while (count > 0)
            {
                ((int*)dest)[0] = ((int*)src)[0];
                ((int*)dest)[1] = ((int*)src)[1];
                ((int*)dest)[2] = ((int*)src)[2];
                ((int*)dest)[3] = ((int*)src)[3];
                dest += 16;
                src += 16;
                count--;
            }

            if ((len & 8) != 0)
            {
                ((int*)dest)[0] = ((int*)src)[0];
                ((int*)dest)[1] = ((int*)src)[1];
                dest += 8;
                src += 8;
            }
            if ((len & 4) != 0)
            {
                ((int*)dest)[0] = ((int*)src)[0];
                dest += 4;
                src += 4;
            }
            if ((len & 2) != 0)
            {
                ((short*)dest)[0] = ((short*)src)[0];
                dest += 2;
                src += 2;
            }
            if ((len & 1) != 0)
                *dest = *src;

            return;
        }

        private static unsafe void SlowCopyBackwards(byte* dest, byte* src, uint len)
        {
            Debug.Assert(len <= int.MaxValue);
            if (len == 0) return;

            for (int i = ((int)len) - 1; i >= 0; i--)
            {
                dest[i] = src[i];
            }
        }

        private static unsafe bool AreOverlapping(byte* dest, byte* src, uint len)
        {
            byte* srcEnd = src + len;
            byte* destEnd = dest + len;
            if (srcEnd >= dest && srcEnd <= destEnd)
            {
                return true;
            }
            return false;
        }

        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [System.Security.SecurityCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemoryCopy(void* source, void* destination, int destinationSizeInBytes, int sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceBytesToCopy));
            }

            Memmove((byte*)destination, (byte*)source, checked((uint)sourceBytesToCopy));
        }
    }
    /// <summary>
    /// Represents a contiguous range of Unicode code points.
    /// </summary>
    /// <remarks>
    /// Currently only the Basic Multilingual Plane is supported.
    /// </remarks>
    public sealed class UnicodeRange
    {
        /// <summary>
        /// Creates a new <see cref="UnicodeRange"/>.
        /// </summary>
        /// <param name="firstCodePoint">The first code point in the range.</param>
        /// <param name="length">The number of code points in the range.</param>
        public UnicodeRange(int firstCodePoint, int length)
        {
            // Parameter checking: the first code point and last code point must
            // lie within the BMP. See http://unicode.org/faq/blocks_ranges.html for more info.
            if (firstCodePoint < 0 || firstCodePoint > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(firstCodePoint));
            }
            if (length < 0 || ((long)firstCodePoint + (long)length > 0x10000))
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            FirstCodePoint = firstCodePoint;
            Length = length;
        }

        /// <summary>
        /// The first code point in this range.
        /// </summary>
        public int FirstCodePoint { get; private set; }

        /// <summary>
        /// The number of code points in this range.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Creates a new <see cref="UnicodeRange"/> from a span of characters.
        /// </summary>
        /// <param name="firstCharacter">The first character in the range.</param>
        /// <param name="lastCharacter">The last character in the range.</param>
        /// <returns>The <see cref="UnicodeRange"/> representing this span.</returns>
        public static UnicodeRange Create(char firstCharacter, char lastCharacter)
        {
            if (lastCharacter < firstCharacter)
            {
                throw new ArgumentOutOfRangeException(nameof(lastCharacter));
            }

            return new UnicodeRange(firstCharacter, 1 + (int)(lastCharacter - firstCharacter));
        }
    }
    /// <summary>
    /// Contains predefined <see cref="UnicodeRange"/> instances which correspond to blocks
    /// from the Unicode 7.0 specification.
    /// </summary>
    public static partial class UnicodeRanges
    {
        /// <summary>
        /// An empty <see cref="UnicodeRange"/>. This range contains no code points.
        /// </summary>
        public static UnicodeRange None { get { return _none ?? CreateEmptyRange(ref _none); } }
        private static UnicodeRange _none;

        /// <summary>
        /// A <see cref="UnicodeRange"/> which contains all characters in the Unicode Basic
        /// Multilingual Plane (U+0000..U+FFFF).
        /// </summary>
        public static UnicodeRange All { get { return _all ?? CreateRange(ref _all, '\u0000', '\uFFFF'); } }
        private static UnicodeRange _all;

        [MethodImpl(MethodImplOptions.NoInlining)] // the caller should be inlined, not this method
        private static UnicodeRange CreateEmptyRange(ref UnicodeRange range)
        {
            // If the range hasn't been created, create it now.
            // It's ok if two threads race and one overwrites the other's 'range' value.
            range = new UnicodeRange(0, 0);
            return range;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // the caller should be inlined, not this method
        private static UnicodeRange CreateRange(ref UnicodeRange range, char first, char last)
        {
            // If the range hasn't been created, create it now.
            // It's ok if two threads race and one overwrites the other's 'range' value.
            range = UnicodeRange.Create(first, last);
            return range;
        }
    }

    public static partial class UnicodeRanges
    {
        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Basic Latin' Unicode block (U+0000..U+007F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0000.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange BasicLatin { get { return _basicLatin ?? CreateRange(ref _basicLatin, first: '\u0000', last: '\u007F'); } }
        private static UnicodeRange _basicLatin;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin-1 Supplement' Unicode block (U+0080..U+00FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0080.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Latin1Supplement { get { return _latin1Supplement ?? CreateRange(ref _latin1Supplement, first: '\u0080', last: '\u00FF'); } }
        private static UnicodeRange _latin1Supplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended-A' Unicode block (U+0100..U+017F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0100.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedA { get { return _latinExtendedA ?? CreateRange(ref _latinExtendedA, first: '\u0100', last: '\u017F'); } }
        private static UnicodeRange _latinExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended-B' Unicode block (U+0180..U+024F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0180.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedB { get { return _latinExtendedB ?? CreateRange(ref _latinExtendedB, first: '\u0180', last: '\u024F'); } }
        private static UnicodeRange _latinExtendedB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'IPA Extensions' Unicode block (U+0250..U+02AF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0250.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange IpaExtensions { get { return _ipaExtensions ?? CreateRange(ref _ipaExtensions, first: '\u0250', last: '\u02AF'); } }
        private static UnicodeRange _ipaExtensions;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Spacing Modifier Letters' Unicode block (U+02B0..U+02FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U02B0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SpacingModifierLetters { get { return _spacingModifierLetters ?? CreateRange(ref _spacingModifierLetters, first: '\u02B0', last: '\u02FF'); } }
        private static UnicodeRange _spacingModifierLetters;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Combining Diacritical Marks' Unicode block (U+0300..U+036F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0300.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CombiningDiacriticalMarks { get { return _combiningDiacriticalMarks ?? CreateRange(ref _combiningDiacriticalMarks, first: '\u0300', last: '\u036F'); } }
        private static UnicodeRange _combiningDiacriticalMarks;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Greek and Coptic' Unicode block (U+0370..U+03FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0370.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange GreekandCoptic { get { return _greekandCoptic ?? CreateRange(ref _greekandCoptic, first: '\u0370', last: '\u03FF'); } }
        private static UnicodeRange _greekandCoptic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cyrillic' Unicode block (U+0400..U+04FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0400.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Cyrillic { get { return _cyrillic ?? CreateRange(ref _cyrillic, first: '\u0400', last: '\u04FF'); } }
        private static UnicodeRange _cyrillic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cyrillic Supplement' Unicode block (U+0500..U+052F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0500.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CyrillicSupplement { get { return _cyrillicSupplement ?? CreateRange(ref _cyrillicSupplement, first: '\u0500', last: '\u052F'); } }
        private static UnicodeRange _cyrillicSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Armenian' Unicode block (U+0530..U+058F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0530.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Armenian { get { return _armenian ?? CreateRange(ref _armenian, first: '\u0530', last: '\u058F'); } }
        private static UnicodeRange _armenian;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hebrew' Unicode block (U+0590..U+05FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0590.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Hebrew { get { return _hebrew ?? CreateRange(ref _hebrew, first: '\u0590', last: '\u05FF'); } }
        private static UnicodeRange _hebrew;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arabic' Unicode block (U+0600..U+06FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0600.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Arabic { get { return _arabic ?? CreateRange(ref _arabic, first: '\u0600', last: '\u06FF'); } }
        private static UnicodeRange _arabic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Syriac' Unicode block (U+0700..U+074F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0700.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Syriac { get { return _syriac ?? CreateRange(ref _syriac, first: '\u0700', last: '\u074F'); } }
        private static UnicodeRange _syriac;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arabic Supplement' Unicode block (U+0750..U+077F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0750.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ArabicSupplement { get { return _arabicSupplement ?? CreateRange(ref _arabicSupplement, first: '\u0750', last: '\u077F'); } }
        private static UnicodeRange _arabicSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Thaana' Unicode block (U+0780..U+07BF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0780.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Thaana { get { return _thaana ?? CreateRange(ref _thaana, first: '\u0780', last: '\u07BF'); } }
        private static UnicodeRange _thaana;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'NKo' Unicode block (U+07C0..U+07FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U07C0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange NKo { get { return _nKo ?? CreateRange(ref _nKo, first: '\u07C0', last: '\u07FF'); } }
        private static UnicodeRange _nKo;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Samaritan' Unicode block (U+0800..U+083F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0800.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Samaritan { get { return _samaritan ?? CreateRange(ref _samaritan, first: '\u0800', last: '\u083F'); } }
        private static UnicodeRange _samaritan;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Mandaic' Unicode block (U+0840..U+085F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0840.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Mandaic { get { return _mandaic ?? CreateRange(ref _mandaic, first: '\u0840', last: '\u085F'); } }
        private static UnicodeRange _mandaic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arabic Extended-A' Unicode block (U+08A0..U+08FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U08A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ArabicExtendedA { get { return _arabicExtendedA ?? CreateRange(ref _arabicExtendedA, first: '\u08A0', last: '\u08FF'); } }
        private static UnicodeRange _arabicExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Devanagari' Unicode block (U+0900..U+097F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0900.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Devanagari { get { return _devanagari ?? CreateRange(ref _devanagari, first: '\u0900', last: '\u097F'); } }
        private static UnicodeRange _devanagari;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Bengali' Unicode block (U+0980..U+09FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0980.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Bengali { get { return _bengali ?? CreateRange(ref _bengali, first: '\u0980', last: '\u09FF'); } }
        private static UnicodeRange _bengali;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Gurmukhi' Unicode block (U+0A00..U+0A7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0A00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Gurmukhi { get { return _gurmukhi ?? CreateRange(ref _gurmukhi, first: '\u0A00', last: '\u0A7F'); } }
        private static UnicodeRange _gurmukhi;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Gujarati' Unicode block (U+0A80..U+0AFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0A80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Gujarati { get { return _gujarati ?? CreateRange(ref _gujarati, first: '\u0A80', last: '\u0AFF'); } }
        private static UnicodeRange _gujarati;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Oriya' Unicode block (U+0B00..U+0B7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0B00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Oriya { get { return _oriya ?? CreateRange(ref _oriya, first: '\u0B00', last: '\u0B7F'); } }
        private static UnicodeRange _oriya;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tamil' Unicode block (U+0B80..U+0BFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0B80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Tamil { get { return _tamil ?? CreateRange(ref _tamil, first: '\u0B80', last: '\u0BFF'); } }
        private static UnicodeRange _tamil;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Telugu' Unicode block (U+0C00..U+0C7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0C00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Telugu { get { return _telugu ?? CreateRange(ref _telugu, first: '\u0C00', last: '\u0C7F'); } }
        private static UnicodeRange _telugu;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Kannada' Unicode block (U+0C80..U+0CFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0C80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Kannada { get { return _kannada ?? CreateRange(ref _kannada, first: '\u0C80', last: '\u0CFF'); } }
        private static UnicodeRange _kannada;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Malayalam' Unicode block (U+0D00..U+0D7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0D00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Malayalam { get { return _malayalam ?? CreateRange(ref _malayalam, first: '\u0D00', last: '\u0D7F'); } }
        private static UnicodeRange _malayalam;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Sinhala' Unicode block (U+0D80..U+0DFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0D80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Sinhala { get { return _sinhala ?? CreateRange(ref _sinhala, first: '\u0D80', last: '\u0DFF'); } }
        private static UnicodeRange _sinhala;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Thai' Unicode block (U+0E00..U+0E7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0E00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Thai { get { return _thai ?? CreateRange(ref _thai, first: '\u0E00', last: '\u0E7F'); } }
        private static UnicodeRange _thai;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Lao' Unicode block (U+0E80..U+0EFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0E80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Lao { get { return _lao ?? CreateRange(ref _lao, first: '\u0E80', last: '\u0EFF'); } }
        private static UnicodeRange _lao;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tibetan' Unicode block (U+0F00..U+0FFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U0F00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Tibetan { get { return _tibetan ?? CreateRange(ref _tibetan, first: '\u0F00', last: '\u0FFF'); } }
        private static UnicodeRange _tibetan;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Myanmar' Unicode block (U+1000..U+109F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1000.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Myanmar { get { return _myanmar ?? CreateRange(ref _myanmar, first: '\u1000', last: '\u109F'); } }
        private static UnicodeRange _myanmar;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Georgian' Unicode block (U+10A0..U+10FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U10A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Georgian { get { return _georgian ?? CreateRange(ref _georgian, first: '\u10A0', last: '\u10FF'); } }
        private static UnicodeRange _georgian;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hangul Jamo' Unicode block (U+1100..U+11FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1100.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HangulJamo { get { return _hangulJamo ?? CreateRange(ref _hangulJamo, first: '\u1100', last: '\u11FF'); } }
        private static UnicodeRange _hangulJamo;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ethiopic' Unicode block (U+1200..U+137F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1200.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Ethiopic { get { return _ethiopic ?? CreateRange(ref _ethiopic, first: '\u1200', last: '\u137F'); } }
        private static UnicodeRange _ethiopic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ethiopic Supplement' Unicode block (U+1380..U+139F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1380.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange EthiopicSupplement { get { return _ethiopicSupplement ?? CreateRange(ref _ethiopicSupplement, first: '\u1380', last: '\u139F'); } }
        private static UnicodeRange _ethiopicSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cherokee' Unicode block (U+13A0..U+13FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U13A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Cherokee { get { return _cherokee ?? CreateRange(ref _cherokee, first: '\u13A0', last: '\u13FF'); } }
        private static UnicodeRange _cherokee;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Unified Canadian Aboriginal Syllabics' Unicode block (U+1400..U+167F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1400.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange UnifiedCanadianAboriginalSyllabics { get { return _unifiedCanadianAboriginalSyllabics ?? CreateRange(ref _unifiedCanadianAboriginalSyllabics, first: '\u1400', last: '\u167F'); } }
        private static UnicodeRange _unifiedCanadianAboriginalSyllabics;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ogham' Unicode block (U+1680..U+169F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1680.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Ogham { get { return _ogham ?? CreateRange(ref _ogham, first: '\u1680', last: '\u169F'); } }
        private static UnicodeRange _ogham;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Runic' Unicode block (U+16A0..U+16FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U16A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Runic { get { return _runic ?? CreateRange(ref _runic, first: '\u16A0', last: '\u16FF'); } }
        private static UnicodeRange _runic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tagalog' Unicode block (U+1700..U+171F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1700.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Tagalog { get { return _tagalog ?? CreateRange(ref _tagalog, first: '\u1700', last: '\u171F'); } }
        private static UnicodeRange _tagalog;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hanunoo' Unicode block (U+1720..U+173F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1720.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Hanunoo { get { return _hanunoo ?? CreateRange(ref _hanunoo, first: '\u1720', last: '\u173F'); } }
        private static UnicodeRange _hanunoo;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Buhid' Unicode block (U+1740..U+175F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1740.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Buhid { get { return _buhid ?? CreateRange(ref _buhid, first: '\u1740', last: '\u175F'); } }
        private static UnicodeRange _buhid;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tagbanwa' Unicode block (U+1760..U+177F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1760.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Tagbanwa { get { return _tagbanwa ?? CreateRange(ref _tagbanwa, first: '\u1760', last: '\u177F'); } }
        private static UnicodeRange _tagbanwa;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Khmer' Unicode block (U+1780..U+17FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1780.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Khmer { get { return _khmer ?? CreateRange(ref _khmer, first: '\u1780', last: '\u17FF'); } }
        private static UnicodeRange _khmer;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Mongolian' Unicode block (U+1800..U+18AF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1800.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Mongolian { get { return _mongolian ?? CreateRange(ref _mongolian, first: '\u1800', last: '\u18AF'); } }
        private static UnicodeRange _mongolian;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Unified Canadian Aboriginal Syllabics Extended' Unicode block (U+18B0..U+18FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U18B0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange UnifiedCanadianAboriginalSyllabicsExtended { get { return _unifiedCanadianAboriginalSyllabicsExtended ?? CreateRange(ref _unifiedCanadianAboriginalSyllabicsExtended, first: '\u18B0', last: '\u18FF'); } }
        private static UnicodeRange _unifiedCanadianAboriginalSyllabicsExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Limbu' Unicode block (U+1900..U+194F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1900.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Limbu { get { return _limbu ?? CreateRange(ref _limbu, first: '\u1900', last: '\u194F'); } }
        private static UnicodeRange _limbu;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tai Le' Unicode block (U+1950..U+197F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1950.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange TaiLe { get { return _taiLe ?? CreateRange(ref _taiLe, first: '\u1950', last: '\u197F'); } }
        private static UnicodeRange _taiLe;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'New Tai Lue' Unicode block (U+1980..U+19DF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1980.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange NewTaiLue { get { return _newTaiLue ?? CreateRange(ref _newTaiLue, first: '\u1980', last: '\u19DF'); } }
        private static UnicodeRange _newTaiLue;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Khmer Symbols' Unicode block (U+19E0..U+19FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U19E0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange KhmerSymbols { get { return _khmerSymbols ?? CreateRange(ref _khmerSymbols, first: '\u19E0', last: '\u19FF'); } }
        private static UnicodeRange _khmerSymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Buginese' Unicode block (U+1A00..U+1A1F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1A00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Buginese { get { return _buginese ?? CreateRange(ref _buginese, first: '\u1A00', last: '\u1A1F'); } }
        private static UnicodeRange _buginese;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tai Tham' Unicode block (U+1A20..U+1AAF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1A20.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange TaiTham { get { return _taiTham ?? CreateRange(ref _taiTham, first: '\u1A20', last: '\u1AAF'); } }
        private static UnicodeRange _taiTham;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Combining Diacritical Marks Extended' Unicode block (U+1AB0..U+1AFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1AB0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CombiningDiacriticalMarksExtended { get { return _combiningDiacriticalMarksExtended ?? CreateRange(ref _combiningDiacriticalMarksExtended, first: '\u1AB0', last: '\u1AFF'); } }
        private static UnicodeRange _combiningDiacriticalMarksExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Balinese' Unicode block (U+1B00..U+1B7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1B00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Balinese { get { return _balinese ?? CreateRange(ref _balinese, first: '\u1B00', last: '\u1B7F'); } }
        private static UnicodeRange _balinese;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Sundanese' Unicode block (U+1B80..U+1BBF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1B80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Sundanese { get { return _sundanese ?? CreateRange(ref _sundanese, first: '\u1B80', last: '\u1BBF'); } }
        private static UnicodeRange _sundanese;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Batak' Unicode block (U+1BC0..U+1BFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1BC0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Batak { get { return _batak ?? CreateRange(ref _batak, first: '\u1BC0', last: '\u1BFF'); } }
        private static UnicodeRange _batak;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Lepcha' Unicode block (U+1C00..U+1C4F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1C00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Lepcha { get { return _lepcha ?? CreateRange(ref _lepcha, first: '\u1C00', last: '\u1C4F'); } }
        private static UnicodeRange _lepcha;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ol Chiki' Unicode block (U+1C50..U+1C7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1C50.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange OlChiki { get { return _olChiki ?? CreateRange(ref _olChiki, first: '\u1C50', last: '\u1C7F'); } }
        private static UnicodeRange _olChiki;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Sundanese Supplement' Unicode block (U+1CC0..U+1CCF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1CC0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SundaneseSupplement { get { return _sundaneseSupplement ?? CreateRange(ref _sundaneseSupplement, first: '\u1CC0', last: '\u1CCF'); } }
        private static UnicodeRange _sundaneseSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Vedic Extensions' Unicode block (U+1CD0..U+1CFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1CD0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange VedicExtensions { get { return _vedicExtensions ?? CreateRange(ref _vedicExtensions, first: '\u1CD0', last: '\u1CFF'); } }
        private static UnicodeRange _vedicExtensions;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Phonetic Extensions' Unicode block (U+1D00..U+1D7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1D00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange PhoneticExtensions { get { return _phoneticExtensions ?? CreateRange(ref _phoneticExtensions, first: '\u1D00', last: '\u1D7F'); } }
        private static UnicodeRange _phoneticExtensions;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Phonetic Extensions Supplement' Unicode block (U+1D80..U+1DBF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1D80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange PhoneticExtensionsSupplement { get { return _phoneticExtensionsSupplement ?? CreateRange(ref _phoneticExtensionsSupplement, first: '\u1D80', last: '\u1DBF'); } }
        private static UnicodeRange _phoneticExtensionsSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Combining Diacritical Marks Supplement' Unicode block (U+1DC0..U+1DFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1DC0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CombiningDiacriticalMarksSupplement { get { return _combiningDiacriticalMarksSupplement ?? CreateRange(ref _combiningDiacriticalMarksSupplement, first: '\u1DC0', last: '\u1DFF'); } }
        private static UnicodeRange _combiningDiacriticalMarksSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended Additional' Unicode block (U+1E00..U+1EFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1E00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedAdditional { get { return _latinExtendedAdditional ?? CreateRange(ref _latinExtendedAdditional, first: '\u1E00', last: '\u1EFF'); } }
        private static UnicodeRange _latinExtendedAdditional;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Greek Extended' Unicode block (U+1F00..U+1FFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U1F00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange GreekExtended { get { return _greekExtended ?? CreateRange(ref _greekExtended, first: '\u1F00', last: '\u1FFF'); } }
        private static UnicodeRange _greekExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'General Punctuation' Unicode block (U+2000..U+206F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2000.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange GeneralPunctuation { get { return _generalPunctuation ?? CreateRange(ref _generalPunctuation, first: '\u2000', last: '\u206F'); } }
        private static UnicodeRange _generalPunctuation;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Superscripts and Subscripts' Unicode block (U+2070..U+209F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2070.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SuperscriptsandSubscripts { get { return _superscriptsandSubscripts ?? CreateRange(ref _superscriptsandSubscripts, first: '\u2070', last: '\u209F'); } }
        private static UnicodeRange _superscriptsandSubscripts;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Currency Symbols' Unicode block (U+20A0..U+20CF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U20A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CurrencySymbols { get { return _currencySymbols ?? CreateRange(ref _currencySymbols, first: '\u20A0', last: '\u20CF'); } }
        private static UnicodeRange _currencySymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Combining Diacritical Marks for Symbols' Unicode block (U+20D0..U+20FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U20D0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CombiningDiacriticalMarksforSymbols { get { return _combiningDiacriticalMarksforSymbols ?? CreateRange(ref _combiningDiacriticalMarksforSymbols, first: '\u20D0', last: '\u20FF'); } }
        private static UnicodeRange _combiningDiacriticalMarksforSymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Letterlike Symbols' Unicode block (U+2100..U+214F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2100.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LetterlikeSymbols { get { return _letterlikeSymbols ?? CreateRange(ref _letterlikeSymbols, first: '\u2100', last: '\u214F'); } }
        private static UnicodeRange _letterlikeSymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Number Forms' Unicode block (U+2150..U+218F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2150.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange NumberForms { get { return _numberForms ?? CreateRange(ref _numberForms, first: '\u2150', last: '\u218F'); } }
        private static UnicodeRange _numberForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arrows' Unicode block (U+2190..U+21FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2190.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Arrows { get { return _arrows ?? CreateRange(ref _arrows, first: '\u2190', last: '\u21FF'); } }
        private static UnicodeRange _arrows;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Mathematical Operators' Unicode block (U+2200..U+22FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2200.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MathematicalOperators { get { return _mathematicalOperators ?? CreateRange(ref _mathematicalOperators, first: '\u2200', last: '\u22FF'); } }
        private static UnicodeRange _mathematicalOperators;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Miscellaneous Technical' Unicode block (U+2300..U+23FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2300.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MiscellaneousTechnical { get { return _miscellaneousTechnical ?? CreateRange(ref _miscellaneousTechnical, first: '\u2300', last: '\u23FF'); } }
        private static UnicodeRange _miscellaneousTechnical;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Control Pictures' Unicode block (U+2400..U+243F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2400.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ControlPictures { get { return _controlPictures ?? CreateRange(ref _controlPictures, first: '\u2400', last: '\u243F'); } }
        private static UnicodeRange _controlPictures;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Optical Character Recognition' Unicode block (U+2440..U+245F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2440.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange OpticalCharacterRecognition { get { return _opticalCharacterRecognition ?? CreateRange(ref _opticalCharacterRecognition, first: '\u2440', last: '\u245F'); } }
        private static UnicodeRange _opticalCharacterRecognition;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Enclosed Alphanumerics' Unicode block (U+2460..U+24FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2460.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange EnclosedAlphanumerics { get { return _enclosedAlphanumerics ?? CreateRange(ref _enclosedAlphanumerics, first: '\u2460', last: '\u24FF'); } }
        private static UnicodeRange _enclosedAlphanumerics;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Box Drawing' Unicode block (U+2500..U+257F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2500.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange BoxDrawing { get { return _boxDrawing ?? CreateRange(ref _boxDrawing, first: '\u2500', last: '\u257F'); } }
        private static UnicodeRange _boxDrawing;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Block Elements' Unicode block (U+2580..U+259F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2580.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange BlockElements { get { return _blockElements ?? CreateRange(ref _blockElements, first: '\u2580', last: '\u259F'); } }
        private static UnicodeRange _blockElements;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Geometric Shapes' Unicode block (U+25A0..U+25FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U25A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange GeometricShapes { get { return _geometricShapes ?? CreateRange(ref _geometricShapes, first: '\u25A0', last: '\u25FF'); } }
        private static UnicodeRange _geometricShapes;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Miscellaneous Symbols' Unicode block (U+2600..U+26FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2600.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MiscellaneousSymbols { get { return _miscellaneousSymbols ?? CreateRange(ref _miscellaneousSymbols, first: '\u2600', last: '\u26FF'); } }
        private static UnicodeRange _miscellaneousSymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Dingbats' Unicode block (U+2700..U+27BF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2700.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Dingbats { get { return _dingbats ?? CreateRange(ref _dingbats, first: '\u2700', last: '\u27BF'); } }
        private static UnicodeRange _dingbats;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Miscellaneous Mathematical Symbols-A' Unicode block (U+27C0..U+27EF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U27C0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MiscellaneousMathematicalSymbolsA { get { return _miscellaneousMathematicalSymbolsA ?? CreateRange(ref _miscellaneousMathematicalSymbolsA, first: '\u27C0', last: '\u27EF'); } }
        private static UnicodeRange _miscellaneousMathematicalSymbolsA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Supplemental Arrows-A' Unicode block (U+27F0..U+27FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U27F0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SupplementalArrowsA { get { return _supplementalArrowsA ?? CreateRange(ref _supplementalArrowsA, first: '\u27F0', last: '\u27FF'); } }
        private static UnicodeRange _supplementalArrowsA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Braille Patterns' Unicode block (U+2800..U+28FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2800.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange BraillePatterns { get { return _braillePatterns ?? CreateRange(ref _braillePatterns, first: '\u2800', last: '\u28FF'); } }
        private static UnicodeRange _braillePatterns;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Supplemental Arrows-B' Unicode block (U+2900..U+297F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2900.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SupplementalArrowsB { get { return _supplementalArrowsB ?? CreateRange(ref _supplementalArrowsB, first: '\u2900', last: '\u297F'); } }
        private static UnicodeRange _supplementalArrowsB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Miscellaneous Mathematical Symbols-B' Unicode block (U+2980..U+29FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2980.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MiscellaneousMathematicalSymbolsB { get { return _miscellaneousMathematicalSymbolsB ?? CreateRange(ref _miscellaneousMathematicalSymbolsB, first: '\u2980', last: '\u29FF'); } }
        private static UnicodeRange _miscellaneousMathematicalSymbolsB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Supplemental Mathematical Operators' Unicode block (U+2A00..U+2AFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2A00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SupplementalMathematicalOperators { get { return _supplementalMathematicalOperators ?? CreateRange(ref _supplementalMathematicalOperators, first: '\u2A00', last: '\u2AFF'); } }
        private static UnicodeRange _supplementalMathematicalOperators;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Miscellaneous Symbols and Arrows' Unicode block (U+2B00..U+2BFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2B00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MiscellaneousSymbolsandArrows { get { return _miscellaneousSymbolsandArrows ?? CreateRange(ref _miscellaneousSymbolsandArrows, first: '\u2B00', last: '\u2BFF'); } }
        private static UnicodeRange _miscellaneousSymbolsandArrows;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Glagolitic' Unicode block (U+2C00..U+2C5F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2C00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Glagolitic { get { return _glagolitic ?? CreateRange(ref _glagolitic, first: '\u2C00', last: '\u2C5F'); } }
        private static UnicodeRange _glagolitic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended-C' Unicode block (U+2C60..U+2C7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2C60.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedC { get { return _latinExtendedC ?? CreateRange(ref _latinExtendedC, first: '\u2C60', last: '\u2C7F'); } }
        private static UnicodeRange _latinExtendedC;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Coptic' Unicode block (U+2C80..U+2CFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2C80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Coptic { get { return _coptic ?? CreateRange(ref _coptic, first: '\u2C80', last: '\u2CFF'); } }
        private static UnicodeRange _coptic;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Georgian Supplement' Unicode block (U+2D00..U+2D2F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2D00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange GeorgianSupplement { get { return _georgianSupplement ?? CreateRange(ref _georgianSupplement, first: '\u2D00', last: '\u2D2F'); } }
        private static UnicodeRange _georgianSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tifinagh' Unicode block (U+2D30..U+2D7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2D30.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Tifinagh { get { return _tifinagh ?? CreateRange(ref _tifinagh, first: '\u2D30', last: '\u2D7F'); } }
        private static UnicodeRange _tifinagh;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ethiopic Extended' Unicode block (U+2D80..U+2DDF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2D80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange EthiopicExtended { get { return _ethiopicExtended ?? CreateRange(ref _ethiopicExtended, first: '\u2D80', last: '\u2DDF'); } }
        private static UnicodeRange _ethiopicExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cyrillic Extended-A' Unicode block (U+2DE0..U+2DFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2DE0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CyrillicExtendedA { get { return _cyrillicExtendedA ?? CreateRange(ref _cyrillicExtendedA, first: '\u2DE0', last: '\u2DFF'); } }
        private static UnicodeRange _cyrillicExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Supplemental Punctuation' Unicode block (U+2E00..U+2E7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2E00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SupplementalPunctuation { get { return _supplementalPunctuation ?? CreateRange(ref _supplementalPunctuation, first: '\u2E00', last: '\u2E7F'); } }
        private static UnicodeRange _supplementalPunctuation;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Radicals Supplement' Unicode block (U+2E80..U+2EFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2E80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkRadicalsSupplement { get { return _cjkRadicalsSupplement ?? CreateRange(ref _cjkRadicalsSupplement, first: '\u2E80', last: '\u2EFF'); } }
        private static UnicodeRange _cjkRadicalsSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Kangxi Radicals' Unicode block (U+2F00..U+2FDF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2F00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange KangxiRadicals { get { return _kangxiRadicals ?? CreateRange(ref _kangxiRadicals, first: '\u2F00', last: '\u2FDF'); } }
        private static UnicodeRange _kangxiRadicals;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ideographic Description Characters' Unicode block (U+2FF0..U+2FFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U2FF0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange IdeographicDescriptionCharacters { get { return _ideographicDescriptionCharacters ?? CreateRange(ref _ideographicDescriptionCharacters, first: '\u2FF0', last: '\u2FFF'); } }
        private static UnicodeRange _ideographicDescriptionCharacters;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Symbols and Punctuation' Unicode block (U+3000..U+303F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3000.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkSymbolsandPunctuation { get { return _cjkSymbolsandPunctuation ?? CreateRange(ref _cjkSymbolsandPunctuation, first: '\u3000', last: '\u303F'); } }
        private static UnicodeRange _cjkSymbolsandPunctuation;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hiragana' Unicode block (U+3040..U+309F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3040.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Hiragana { get { return _hiragana ?? CreateRange(ref _hiragana, first: '\u3040', last: '\u309F'); } }
        private static UnicodeRange _hiragana;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Katakana' Unicode block (U+30A0..U+30FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U30A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Katakana { get { return _katakana ?? CreateRange(ref _katakana, first: '\u30A0', last: '\u30FF'); } }
        private static UnicodeRange _katakana;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Bopomofo' Unicode block (U+3100..U+312F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3100.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Bopomofo { get { return _bopomofo ?? CreateRange(ref _bopomofo, first: '\u3100', last: '\u312F'); } }
        private static UnicodeRange _bopomofo;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hangul Compatibility Jamo' Unicode block (U+3130..U+318F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3130.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HangulCompatibilityJamo { get { return _hangulCompatibilityJamo ?? CreateRange(ref _hangulCompatibilityJamo, first: '\u3130', last: '\u318F'); } }
        private static UnicodeRange _hangulCompatibilityJamo;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Kanbun' Unicode block (U+3190..U+319F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3190.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Kanbun { get { return _kanbun ?? CreateRange(ref _kanbun, first: '\u3190', last: '\u319F'); } }
        private static UnicodeRange _kanbun;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Bopomofo Extended' Unicode block (U+31A0..U+31BF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U31A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange BopomofoExtended { get { return _bopomofoExtended ?? CreateRange(ref _bopomofoExtended, first: '\u31A0', last: '\u31BF'); } }
        private static UnicodeRange _bopomofoExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Strokes' Unicode block (U+31C0..U+31EF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U31C0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkStrokes { get { return _cjkStrokes ?? CreateRange(ref _cjkStrokes, first: '\u31C0', last: '\u31EF'); } }
        private static UnicodeRange _cjkStrokes;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Katakana Phonetic Extensions' Unicode block (U+31F0..U+31FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U31F0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange KatakanaPhoneticExtensions { get { return _katakanaPhoneticExtensions ?? CreateRange(ref _katakanaPhoneticExtensions, first: '\u31F0', last: '\u31FF'); } }
        private static UnicodeRange _katakanaPhoneticExtensions;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Enclosed CJK Letters and Months' Unicode block (U+3200..U+32FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3200.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange EnclosedCjkLettersandMonths { get { return _enclosedCjkLettersandMonths ?? CreateRange(ref _enclosedCjkLettersandMonths, first: '\u3200', last: '\u32FF'); } }
        private static UnicodeRange _enclosedCjkLettersandMonths;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Compatibility' Unicode block (U+3300..U+33FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3300.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkCompatibility { get { return _cjkCompatibility ?? CreateRange(ref _cjkCompatibility, first: '\u3300', last: '\u33FF'); } }
        private static UnicodeRange _cjkCompatibility;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Unified Ideographs Extension A' Unicode block (U+3400..U+4DBF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U3400.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkUnifiedIdeographsExtensionA { get { return _cjkUnifiedIdeographsExtensionA ?? CreateRange(ref _cjkUnifiedIdeographsExtensionA, first: '\u3400', last: '\u4DBF'); } }
        private static UnicodeRange _cjkUnifiedIdeographsExtensionA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Yijing Hexagram Symbols' Unicode block (U+4DC0..U+4DFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U4DC0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange YijingHexagramSymbols { get { return _yijingHexagramSymbols ?? CreateRange(ref _yijingHexagramSymbols, first: '\u4DC0', last: '\u4DFF'); } }
        private static UnicodeRange _yijingHexagramSymbols;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Unified Ideographs' Unicode block (U+4E00..U+9FFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/U4E00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkUnifiedIdeographs { get { return _cjkUnifiedIdeographs ?? CreateRange(ref _cjkUnifiedIdeographs, first: '\u4E00', last: '\u9FFF'); } }
        private static UnicodeRange _cjkUnifiedIdeographs;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Yi Syllables' Unicode block (U+A000..U+A48F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA000.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange YiSyllables { get { return _yiSyllables ?? CreateRange(ref _yiSyllables, first: '\uA000', last: '\uA48F'); } }
        private static UnicodeRange _yiSyllables;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Yi Radicals' Unicode block (U+A490..U+A4CF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA490.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange YiRadicals { get { return _yiRadicals ?? CreateRange(ref _yiRadicals, first: '\uA490', last: '\uA4CF'); } }
        private static UnicodeRange _yiRadicals;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Lisu' Unicode block (U+A4D0..U+A4FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA4D0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Lisu { get { return _lisu ?? CreateRange(ref _lisu, first: '\uA4D0', last: '\uA4FF'); } }
        private static UnicodeRange _lisu;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Vai' Unicode block (U+A500..U+A63F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA500.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Vai { get { return _vai ?? CreateRange(ref _vai, first: '\uA500', last: '\uA63F'); } }
        private static UnicodeRange _vai;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cyrillic Extended-B' Unicode block (U+A640..U+A69F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA640.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CyrillicExtendedB { get { return _cyrillicExtendedB ?? CreateRange(ref _cyrillicExtendedB, first: '\uA640', last: '\uA69F'); } }
        private static UnicodeRange _cyrillicExtendedB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Bamum' Unicode block (U+A6A0..U+A6FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA6A0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Bamum { get { return _bamum ?? CreateRange(ref _bamum, first: '\uA6A0', last: '\uA6FF'); } }
        private static UnicodeRange _bamum;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Modifier Tone Letters' Unicode block (U+A700..U+A71F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA700.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ModifierToneLetters { get { return _modifierToneLetters ?? CreateRange(ref _modifierToneLetters, first: '\uA700', last: '\uA71F'); } }
        private static UnicodeRange _modifierToneLetters;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended-D' Unicode block (U+A720..U+A7FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA720.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedD { get { return _latinExtendedD ?? CreateRange(ref _latinExtendedD, first: '\uA720', last: '\uA7FF'); } }
        private static UnicodeRange _latinExtendedD;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Syloti Nagri' Unicode block (U+A800..U+A82F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA800.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SylotiNagri { get { return _sylotiNagri ?? CreateRange(ref _sylotiNagri, first: '\uA800', last: '\uA82F'); } }
        private static UnicodeRange _sylotiNagri;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Common Indic Number Forms' Unicode block (U+A830..U+A83F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA830.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CommonIndicNumberForms { get { return _commonIndicNumberForms ?? CreateRange(ref _commonIndicNumberForms, first: '\uA830', last: '\uA83F'); } }
        private static UnicodeRange _commonIndicNumberForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Phags-pa' Unicode block (U+A840..U+A87F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA840.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Phagspa { get { return _phagspa ?? CreateRange(ref _phagspa, first: '\uA840', last: '\uA87F'); } }
        private static UnicodeRange _phagspa;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Saurashtra' Unicode block (U+A880..U+A8DF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA880.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Saurashtra { get { return _saurashtra ?? CreateRange(ref _saurashtra, first: '\uA880', last: '\uA8DF'); } }
        private static UnicodeRange _saurashtra;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Devanagari Extended' Unicode block (U+A8E0..U+A8FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA8E0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange DevanagariExtended { get { return _devanagariExtended ?? CreateRange(ref _devanagariExtended, first: '\uA8E0', last: '\uA8FF'); } }
        private static UnicodeRange _devanagariExtended;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Kayah Li' Unicode block (U+A900..U+A92F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA900.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange KayahLi { get { return _kayahLi ?? CreateRange(ref _kayahLi, first: '\uA900', last: '\uA92F'); } }
        private static UnicodeRange _kayahLi;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Rejang' Unicode block (U+A930..U+A95F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA930.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Rejang { get { return _rejang ?? CreateRange(ref _rejang, first: '\uA930', last: '\uA95F'); } }
        private static UnicodeRange _rejang;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hangul Jamo Extended-A' Unicode block (U+A960..U+A97F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA960.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HangulJamoExtendedA { get { return _hangulJamoExtendedA ?? CreateRange(ref _hangulJamoExtendedA, first: '\uA960', last: '\uA97F'); } }
        private static UnicodeRange _hangulJamoExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Javanese' Unicode block (U+A980..U+A9DF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA980.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Javanese { get { return _javanese ?? CreateRange(ref _javanese, first: '\uA980', last: '\uA9DF'); } }
        private static UnicodeRange _javanese;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Myanmar Extended-B' Unicode block (U+A9E0..U+A9FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UA9E0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MyanmarExtendedB { get { return _myanmarExtendedB ?? CreateRange(ref _myanmarExtendedB, first: '\uA9E0', last: '\uA9FF'); } }
        private static UnicodeRange _myanmarExtendedB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cham' Unicode block (U+AA00..U+AA5F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAA00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Cham { get { return _cham ?? CreateRange(ref _cham, first: '\uAA00', last: '\uAA5F'); } }
        private static UnicodeRange _cham;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Myanmar Extended-A' Unicode block (U+AA60..U+AA7F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAA60.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MyanmarExtendedA { get { return _myanmarExtendedA ?? CreateRange(ref _myanmarExtendedA, first: '\uAA60', last: '\uAA7F'); } }
        private static UnicodeRange _myanmarExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Tai Viet' Unicode block (U+AA80..U+AADF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAA80.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange TaiViet { get { return _taiViet ?? CreateRange(ref _taiViet, first: '\uAA80', last: '\uAADF'); } }
        private static UnicodeRange _taiViet;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Meetei Mayek Extensions' Unicode block (U+AAE0..U+AAFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAAE0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MeeteiMayekExtensions { get { return _meeteiMayekExtensions ?? CreateRange(ref _meeteiMayekExtensions, first: '\uAAE0', last: '\uAAFF'); } }
        private static UnicodeRange _meeteiMayekExtensions;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Ethiopic Extended-A' Unicode block (U+AB00..U+AB2F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAB00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange EthiopicExtendedA { get { return _ethiopicExtendedA ?? CreateRange(ref _ethiopicExtendedA, first: '\uAB00', last: '\uAB2F'); } }
        private static UnicodeRange _ethiopicExtendedA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Latin Extended-E' Unicode block (U+AB30..U+AB6F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAB30.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange LatinExtendedE { get { return _latinExtendedE ?? CreateRange(ref _latinExtendedE, first: '\uAB30', last: '\uAB6F'); } }
        private static UnicodeRange _latinExtendedE;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Cherokee Supplement' Unicode block (U+AB70..U+ABBF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAB70.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CherokeeSupplement { get { return _cherokeeSupplement ?? CreateRange(ref _cherokeeSupplement, first: '\uAB70', last: '\uABBF'); } }
        private static UnicodeRange _cherokeeSupplement;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Meetei Mayek' Unicode block (U+ABC0..U+ABFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UABC0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange MeeteiMayek { get { return _meeteiMayek ?? CreateRange(ref _meeteiMayek, first: '\uABC0', last: '\uABFF'); } }
        private static UnicodeRange _meeteiMayek;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hangul Syllables' Unicode block (U+AC00..U+D7AF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UAC00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HangulSyllables { get { return _hangulSyllables ?? CreateRange(ref _hangulSyllables, first: '\uAC00', last: '\uD7AF'); } }
        private static UnicodeRange _hangulSyllables;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Hangul Jamo Extended-B' Unicode block (U+D7B0..U+D7FF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UD7B0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HangulJamoExtendedB { get { return _hangulJamoExtendedB ?? CreateRange(ref _hangulJamoExtendedB, first: '\uD7B0', last: '\uD7FF'); } }
        private static UnicodeRange _hangulJamoExtendedB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Compatibility Ideographs' Unicode block (U+F900..U+FAFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UF900.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkCompatibilityIdeographs { get { return _cjkCompatibilityIdeographs ?? CreateRange(ref _cjkCompatibilityIdeographs, first: '\uF900', last: '\uFAFF'); } }
        private static UnicodeRange _cjkCompatibilityIdeographs;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Alphabetic Presentation Forms' Unicode block (U+FB00..U+FB4F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFB00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange AlphabeticPresentationForms { get { return _alphabeticPresentationForms ?? CreateRange(ref _alphabeticPresentationForms, first: '\uFB00', last: '\uFB4F'); } }
        private static UnicodeRange _alphabeticPresentationForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arabic Presentation Forms-A' Unicode block (U+FB50..U+FDFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFB50.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ArabicPresentationFormsA { get { return _arabicPresentationFormsA ?? CreateRange(ref _arabicPresentationFormsA, first: '\uFB50', last: '\uFDFF'); } }
        private static UnicodeRange _arabicPresentationFormsA;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Variation Selectors' Unicode block (U+FE00..U+FE0F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange VariationSelectors { get { return _variationSelectors ?? CreateRange(ref _variationSelectors, first: '\uFE00', last: '\uFE0F'); } }
        private static UnicodeRange _variationSelectors;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Vertical Forms' Unicode block (U+FE10..U+FE1F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE10.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange VerticalForms { get { return _verticalForms ?? CreateRange(ref _verticalForms, first: '\uFE10', last: '\uFE1F'); } }
        private static UnicodeRange _verticalForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Combining Half Marks' Unicode block (U+FE20..U+FE2F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE20.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CombiningHalfMarks { get { return _combiningHalfMarks ?? CreateRange(ref _combiningHalfMarks, first: '\uFE20', last: '\uFE2F'); } }
        private static UnicodeRange _combiningHalfMarks;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'CJK Compatibility Forms' Unicode block (U+FE30..U+FE4F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE30.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange CjkCompatibilityForms { get { return _cjkCompatibilityForms ?? CreateRange(ref _cjkCompatibilityForms, first: '\uFE30', last: '\uFE4F'); } }
        private static UnicodeRange _cjkCompatibilityForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Small Form Variants' Unicode block (U+FE50..U+FE6F).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE50.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange SmallFormVariants { get { return _smallFormVariants ?? CreateRange(ref _smallFormVariants, first: '\uFE50', last: '\uFE6F'); } }
        private static UnicodeRange _smallFormVariants;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Arabic Presentation Forms-B' Unicode block (U+FE70..U+FEFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFE70.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange ArabicPresentationFormsB { get { return _arabicPresentationFormsB ?? CreateRange(ref _arabicPresentationFormsB, first: '\uFE70', last: '\uFEFF'); } }
        private static UnicodeRange _arabicPresentationFormsB;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Halfwidth and Fullwidth Forms' Unicode block (U+FF00..U+FFEF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFF00.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange HalfwidthandFullwidthForms { get { return _halfwidthandFullwidthForms ?? CreateRange(ref _halfwidthandFullwidthForms, first: '\uFF00', last: '\uFFEF'); } }
        private static UnicodeRange _halfwidthandFullwidthForms;

        /// <summary>
        /// A <see cref="UnicodeRange"/> corresponding to the 'Specials' Unicode block (U+FFF0..U+FFFF).
        /// </summary>
        /// <remarks>
        /// See http://www.unicode.org/charts/PDF/UFFF0.pdf for the full set of characters in this block.
        /// </remarks>
        public static UnicodeRange Specials { get { return _specials ?? CreateRange(ref _specials, first: '\uFFF0', last: '\uFFFF'); } }
        private static UnicodeRange _specials;
    }

    /// <summary>
    /// Contains helpers for dealing with Unicode code points.
    /// </summary>
    internal static unsafe class UnicodeHelpers
    {
        /// <summary>
        /// Used for invalid Unicode sequences or other unrepresentable values.
        /// </summary>
        private const char UNICODE_REPLACEMENT_CHAR = '\uFFFD';

        /// <summary>
        /// The last code point defined by the Unicode specification.
        /// </summary>
        internal const int UNICODE_LAST_CODEPOINT = 0x10FFFF;

        private static uint[] _definedCharacterBitmap;

        /// <summary>
        /// Helper method which creates a bitmap of all characters which are
        /// defined per the Unicode specification.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static uint[] CreateDefinedCharacterBitmap()
        {
            // The stream should be exactly 8KB in size.
            byte[] buffer = new byte[] { 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127, 0, 0, 0, 0, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 252, 240, 215, 255, 255, 251, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 254, 255, 255, 255, 127, 254, 254, 255, 255, 255, 255, 230, 254, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 7, 31, 0, 255, 255, 255, 223, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 191, 255, 255, 255, 255, 255, 255, 255, 231, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 3, 0, 255, 255, 255, 255, 255, 255, 255, 7, 255, 255, 255, 255, 255, 63, 255, 127, 255, 255, 255, 79, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 31, 0, 0, 0, 0, 0, 248, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 239, 159, 249, 255, 255, 253, 197, 243, 159, 121, 128, 176, 207, 255, 255, 15, 238, 135, 249, 255, 255, 253, 109, 211, 135, 57, 2, 94, 192, 255, 63, 0, 238, 191, 251, 255, 255, 253, 237, 243, 191, 59, 1, 0, 207, 255, 3, 2, 238, 159, 249, 255, 255, 253, 237, 243, 159, 57, 192, 176, 207, 255, 255, 0, 236, 199, 61, 214, 24, 199, 255, 195, 199, 61, 129, 0, 192, 255, 255, 7, 239, 223, 253, 255, 255, 253, 255, 227, 223, 61, 96, 7, 207, 255, 0, 255, 238, 223, 253, 255, 255, 253, 239, 243, 223, 61, 96, 64, 207, 255, 6, 0, 238, 223, 253, 255, 255, 255, 255, 231, 223, 125, 128, 128, 207, 255, 63, 254, 236, 255, 127, 252, 255, 255, 251, 47, 127, 132, 95, 255, 192, 255, 28, 0, 254, 255, 255, 255, 255, 255, 255, 135, 255, 255, 255, 15, 0, 0, 0, 0, 150, 37, 240, 254, 174, 236, 255, 59, 95, 63, 255, 243, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 254, 255, 255, 255, 31, 254, 255, 255, 255, 255, 254, 255, 255, 255, 223, 255, 223, 255, 7, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 191, 32, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 61, 127, 61, 255, 255, 255, 255, 255, 61, 255, 255, 255, 255, 61, 127, 61, 255, 127, 255, 255, 255, 255, 255, 255, 255, 61, 255, 255, 255, 255, 255, 255, 255, 255, 231, 255, 255, 255, 31, 255, 255, 255, 3, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 63, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 254, 255, 255, 31, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 1, 255, 223, 31, 0, 255, 255, 127, 0, 255, 255, 15, 0, 255, 223, 13, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 255, 3, 255, 3, 255, 127, 255, 3, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 7, 255, 255, 255, 255, 255, 255, 255, 255, 63, 0, 255, 255, 255, 127, 255, 15, 255, 15, 241, 255, 255, 255, 255, 63, 31, 0, 255, 255, 255, 255, 255, 15, 255, 255, 255, 3, 255, 199, 255, 255, 255, 255, 255, 255, 255, 207, 255, 255, 255, 255, 255, 255, 255, 127, 255, 255, 255, 159, 255, 3, 255, 3, 255, 63, 255, 127, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 255, 255, 255, 255, 255, 31, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 240, 255, 255, 255, 255, 255, 255, 255, 248, 255, 227, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 255, 0, 255, 255, 255, 255, 127, 3, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 240, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 63, 255, 255, 255, 255, 63, 63, 255, 170, 255, 255, 255, 63, 255, 255, 255, 255, 255, 255, 223, 255, 223, 255, 207, 239, 255, 255, 220, 127, 0, 248, 255, 255, 255, 124, 255, 255, 255, 255, 255, 127, 223, 255, 243, 255, 255, 127, 255, 31, 255, 255, 255, 127, 0, 0, 255, 255, 255, 255, 1, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 7, 255, 255, 255, 255, 127, 0, 0, 0, 255, 7, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 207, 255, 255, 255, 63, 255, 255, 255, 255, 227, 255, 253, 3, 0, 0, 240, 0, 0, 255, 255, 255, 255, 255, 127, 255, 255, 255, 255, 255, 127, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 254, 255, 255, 255, 255, 191, 32, 255, 255, 255, 255, 255, 255, 255, 128, 1, 128, 255, 255, 127, 0, 127, 127, 127, 127, 127, 127, 127, 127, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 7, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 251, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 0, 0, 0, 255, 15, 254, 255, 255, 255, 255, 255, 255, 255, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 224, 255, 255, 255, 255, 63, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127, 255, 255, 255, 255, 255, 7, 255, 255, 255, 255, 15, 0, 255, 255, 255, 255, 255, 127, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 31, 255, 255, 255, 255, 255, 255, 127, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 255, 0, 0, 0, 0, 0, 0, 0, 128, 255, 255, 255, 255, 255, 255, 15, 255, 3, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 31, 192, 255, 3, 255, 255, 255, 63, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 128, 255, 255, 255, 31, 255, 255, 255, 255, 255, 255, 255, 255, 255, 191, 255, 195, 255, 255, 255, 127, 255, 255, 255, 255, 255, 255, 127, 0, 255, 63, 255, 243, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 7, 0, 0, 248, 255, 255, 127, 0, 126, 126, 126, 0, 127, 127, 255, 255, 255, 255, 255, 255, 63, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 255, 3, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 15, 0, 255, 255, 127, 248, 255, 255, 255, 255, 255, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 63, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 3, 0, 0, 0, 0, 127, 0, 248, 224, 255, 255, 127, 95, 219, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 3, 0, 248, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 252, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 255, 63, 255, 255, 255, 3, 255, 255, 255, 255, 255, 255, 247, 255, 127, 15, 223, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 159, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127, 252, 252, 252, 28, 127, 127, 0, 62 };

            var stream = new System.IO.MemoryStream(buffer);//typeof(UnicodeRange).GetTypeInfo().Assembly.GetManifestResourceStream("System.Text.Encodings.Web.Resources.unicode8definedcharacters.bin");

            if (stream == null)
            {
                throw new BadImageFormatException();
            }

            if (stream.Length != 8 * 1024)
            {
                Environment.FailFast("Corrupt data detected.");
            }

            // Read everything in as raw bytes.
            byte[] rawData = new byte[8 * 1024];
            for (int numBytesReadTotal = 0; numBytesReadTotal < rawData.Length;)
            {
                int numBytesReadThisIteration = stream.Read(rawData, numBytesReadTotal, rawData.Length - numBytesReadTotal);
                if (numBytesReadThisIteration == 0)
                {
                    Environment.FailFast("Corrupt data detected.");
                }
                numBytesReadTotal += numBytesReadThisIteration;
            }

            // Finally, convert the byte[] to a uint[].
            // The incoming bytes are little-endian.
            uint[] retVal = new uint[2 * 1024];
            for (int i = 0; i < retVal.Length; i++)
            {
                retVal[i] = (((uint)rawData[4 * i + 3]) << 24)
                    | (((uint)rawData[4 * i + 2]) << 16)
                    | (((uint)rawData[4 * i + 1]) << 8)
                    | (uint)rawData[4 * i];
            }

            // And we're done!
            Volatile.Write(ref _definedCharacterBitmap, retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a bitmap of all characters which are defined per version 7.0.0
        /// of the Unicode specification.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint[] GetDefinedCharacterBitmap()
        {
            return Volatile.Read(ref _definedCharacterBitmap) ?? CreateDefinedCharacterBitmap();
        }

        /// <summary>
        /// Given a UTF-16 character stream, reads the next scalar value from the stream.
        /// Set 'endOfString' to true if 'pChar' points to the last character in the stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetScalarValueFromUtf16(char first, char? second, out bool wasSurrogatePair)
        {
            if (!Char.IsSurrogate(first))
            {
                wasSurrogatePair = false;
                return first;
            }
            return GetScalarValueFromUtf16Slow(first, second, out wasSurrogatePair);
        }

        private static int GetScalarValueFromUtf16Slow(char first, char? second, out bool wasSurrogatePair)
        {
#if DEBUG
            if (!Char.IsSurrogate(first))
            {
                Debug.Assert(false, "This case should've been handled by the fast path.");
                wasSurrogatePair = false;
                return first;
            }
#endif
            if (Char.IsHighSurrogate(first))
            {
                if (second != null)
                {
                    if (Char.IsLowSurrogate(second.Value))
                    {
                        // valid surrogate pair - extract codepoint
                        wasSurrogatePair = true;
                        return GetScalarValueFromUtf16SurrogatePair(first, second.Value);
                    }
                    else
                    {
                        // unmatched surrogate - substitute
                        wasSurrogatePair = false;
                        return UNICODE_REPLACEMENT_CHAR;
                    }
                }
                else
                {
                    // unmatched surrogate - substitute
                    wasSurrogatePair = false;
                    return UNICODE_REPLACEMENT_CHAR;
                }
            }
            else
            {
                // unmatched surrogate - substitute
                Debug.Assert(Char.IsLowSurrogate(first));
                wasSurrogatePair = false;
                return UNICODE_REPLACEMENT_CHAR;
            }
        }

        /// <summary>
        /// Given a UTF-16 character stream, reads the next scalar value from the stream.
        /// Set 'endOfString' to true if 'pChar' points to the last character in the stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetScalarValueFromUtf16(char* pChar, bool endOfString)
        {
            // This method is marked as AggressiveInlining to handle the common case of a non-surrogate
            // character. The surrogate case is handled in the slower fallback code path.
            char thisChar = *pChar;
            return (Char.IsSurrogate(thisChar)) ? GetScalarValueFromUtf16Slow(pChar, endOfString) : thisChar;
        }

        private static int GetScalarValueFromUtf16Slow(char* pChar, bool endOfString)
        {
            char firstChar = pChar[0];

            if (!Char.IsSurrogate(firstChar))
            {
                Debug.Assert(false, "This case should've been handled by the fast path.");
                return firstChar;
            }
            else if (Char.IsHighSurrogate(firstChar))
            {
                if (endOfString)
                {
                    // unmatched surrogate - substitute
                    return UNICODE_REPLACEMENT_CHAR;
                }
                else
                {
                    char secondChar = pChar[1];
                    if (Char.IsLowSurrogate(secondChar))
                    {
                        // valid surrogate pair - extract codepoint
                        return GetScalarValueFromUtf16SurrogatePair(firstChar, secondChar);
                    }
                    else
                    {
                        // unmatched surrogate - substitute
                        return UNICODE_REPLACEMENT_CHAR;
                    }
                }
            }
            else
            {
                // unmatched surrogate - substitute
                Debug.Assert(Char.IsLowSurrogate(firstChar));
                return UNICODE_REPLACEMENT_CHAR;
            }
        }

        private static int GetScalarValueFromUtf16SurrogatePair(char highSurrogate, char lowSurrogate)
        {
            Debug.Assert(Char.IsHighSurrogate(highSurrogate));
            Debug.Assert(Char.IsLowSurrogate(lowSurrogate));

            // See http://www.unicode.org/versions/Unicode6.2.0/ch03.pdf, Table 3.5 for the
            // details of this conversion. We don't use Char.ConvertToUtf32 because its exception
            // handling shows up on the hot path, and our caller has already sanitized the inputs.
            return (lowSurrogate & 0x3ff) | (((highSurrogate & 0x3ff) + (1 << 6)) << 10);
        }

        internal static void GetUtf16SurrogatePairFromAstralScalarValue(int scalar, out char highSurrogate, out char lowSurrogate)
        {
            Debug.Assert(0x10000 <= scalar && scalar <= UNICODE_LAST_CODEPOINT);

            // See http://www.unicode.org/versions/Unicode6.2.0/ch03.pdf, Table 3.5 for the
            // details of this conversion. We don't use Char.ConvertFromUtf32 because its exception
            // handling shows up on the hot path, it allocates temporary strings (which we don't want),
            // and our caller has already sanitized the inputs.

            int x = scalar & 0xFFFF;
            int u = scalar >> 16;
            int w = u - 1;
            highSurrogate = (char)(0xD800 | (w << 6) | (x >> 10));
            lowSurrogate = (char)(0xDC00 | (x & 0x3FF));
        }

        /// <summary>
        /// Given a Unicode scalar value, returns the UTF-8 representation of the value.
        /// The return value's bytes should be popped from the LSB.
        /// </summary>
        internal static int GetUtf8RepresentationForScalarValue(uint scalar)
        {
            Debug.Assert(scalar <= UNICODE_LAST_CODEPOINT);

            // See http://www.unicode.org/versions/Unicode6.2.0/ch03.pdf, Table 3.6 for the
            // details of this conversion. We don't use UTF8Encoding since we're encoding
            // a scalar code point, not a UTF16 character sequence.
            if (scalar <= 0x7f)
            {
                // one byte used: scalar 00000000 0xxxxxxx -> byte sequence 0xxxxxxx
                byte firstByte = (byte)scalar;
                return firstByte;
            }
            else if (scalar <= 0x7ff)
            {
                // two bytes used: scalar 00000yyy yyxxxxxx -> byte sequence 110yyyyy 10xxxxxx
                byte firstByte = (byte)(0xc0 | (scalar >> 6));
                byte secondByteByte = (byte)(0x80 | (scalar & 0x3f));
                return ((secondByteByte << 8) | firstByte);
            }
            else if (scalar <= 0xffff)
            {
                // three bytes used: scalar zzzzyyyy yyxxxxxx -> byte sequence 1110zzzz 10yyyyyy 10xxxxxx
                byte firstByte = (byte)(0xe0 | (scalar >> 12));
                byte secondByte = (byte)(0x80 | ((scalar >> 6) & 0x3f));
                byte thirdByte = (byte)(0x80 | (scalar & 0x3f));
                return ((((thirdByte << 8) | secondByte) << 8) | firstByte);
            }
            else
            {
                // four bytes used: scalar 000uuuuu zzzzyyyy yyxxxxxx -> byte sequence 11110uuu 10uuzzzz 10yyyyyy 10xxxxxx
                byte firstByte = (byte)(0xf0 | (scalar >> 18));
                byte secondByte = (byte)(0x80 | ((scalar >> 12) & 0x3f));
                byte thirdByte = (byte)(0x80 | ((scalar >> 6) & 0x3f));
                byte fourthByte = (byte)(0x80 | (scalar & 0x3f));
                return ((((((fourthByte << 8) | thirdByte) << 8) | secondByte) << 8) | firstByte);
            }
        }

        /// <summary>
        /// Returns a value stating whether a character is defined per version 7.0.0
        /// of the Unicode specification. Certain classes of characters (control chars,
        /// private use, surrogates, some whitespace) are considered "undefined" for
        /// our purposes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsCharacterDefined(char c)
        {
            uint codePoint = (uint)c;
            int index = (int)(codePoint >> 5);
            int offset = (int)(codePoint & 0x1FU);
            return ((GetDefinedCharacterBitmap()[index] >> offset) & 0x1U) != 0;
        }

        /// <summary>
        /// Determines whether the given scalar value is in the supplementary plane and thus
        /// requires 2 characters to be represented in UTF-16 (as a surrogate pair).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSupplementaryCodePoint(int scalar)
        {
            return ((scalar & ~((int)Char.MaxValue)) != 0);
        }
    }
    /// <summary>
    /// An abstraction representing various text encoders. 
    /// </summary>
    /// <remarks>
    /// TextEncoder subclasses can be used to do HTML encoding, URI encoding, and JavaScript encoding. 
    /// Instances of such subclasses can be accessed using <see cref="HtmlEncoder.Default"/>, <see cref="UrlEncoder.Default"/>, and <see cref="JavaScriptEncoder.Default"/>.
    /// </remarks>
    public abstract class TextEncoder
    {
        // The following pragma disables a warning complaining about non-CLS compliant members being abstract, 
        // and wants me to mark the type as non-CLS compliant. 
        // It is true that this type cannot be extended by all CLS compliant languages. 
        // Having said that, if I marked the type as non-CLS all methods that take it as parameter will now have to be marked CLSCompliant(false), 
        // yet consumption of concrete encoders is totally CLS compliant, 
        // as it?s mainly to be done by calling helper methods in TextEncoderExtensions class, 
        // and so I think the warning is a bit too aggressive.  

        /// <summary>
        /// Encodes a Unicode scalar into a buffer.
        /// </summary>
        /// <param name="unicodeScalar">Unicode scalar.</param>
        /// <param name="buffer">The destination of the encoded text.</param>
        /// <param name="bufferLength">Length of the destination <paramref name="buffer"/> in chars.</param>
        /// <param name="numberOfCharactersWritten">Number of characters written to the <paramref name="buffer"/>.</param>
        /// <returns>Returns false if <paramref name="bufferLength"/> is too small to fit the encoded text, otherwise returns true.</returns>
        /// <remarks>This method is seldom called directly. One of the TextEncoder.Encode overloads should be used instead.
        /// Implementations of <see cref="TextEncoder"/> need to be thread safe and stateless.
        /// </remarks>
#pragma warning disable 3011
        public unsafe abstract bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten);

        // all subclasses have the same implementation of this method.
        // but this cannot be made virtual, because it will cause a virtual call to Encodes, and it destroys perf, i.e. makes common scenario 2x slower 

        /// <summary>
        /// Finds index of the first character that needs to be encoded.
        /// </summary>
        /// <param name="text">The text buffer to search.</param>
        /// <param name="textLength">The number of characters in the <paramref name="text"/>.</param>
        /// <returns></returns>
        /// <remarks>This method is seldom called directly. It's used by higher level helper APIs.</remarks>
        public unsafe abstract int FindFirstCharacterToEncode(char* text, int textLength);
#pragma warning restore

        /// <summary>
        /// Determines if a given Unicode scalar will be encoded.
        /// </summary>
        /// <param name="unicodeScalar">Unicode scalar.</param>
        /// <returns>Returns true if the <paramref name="unicodeScalar"/> will be encoded by this encoder, otherwise returns false.</returns>
        public abstract bool WillEncode(int unicodeScalar);

        // this could be a field, but I am trying to make the abstraction pure.

        /// <summary>
        /// Maximum number of characters that this encoder can generate for each input character.
        /// </summary>
        public abstract int MaxOutputCharactersPerInputCharacter { get; }

        /// <summary>
        /// Encodes the supplied string and returns the encoded text as a new string.
        /// </summary>
        /// <param name="value">String to encode.</param>
        /// <returns>Encoded string.</returns>
        public virtual string Encode(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            unsafe
            {
                fixed (char* valuePointer = value)
                {
                    int firstCharacterToEncode = FindFirstCharacterToEncode(valuePointer, value.Length);

                    if (firstCharacterToEncode == -1)
                    {
                        return value;
                    }

                    int bufferSize = MaxOutputCharactersPerInputCharacter * value.Length;

                    string result;
                    if (bufferSize < 1024)
                    {
                        char* wholebuffer = stackalloc char[bufferSize];
                        int totalWritten = EncodeIntoBuffer(wholebuffer, bufferSize, valuePointer, value.Length, firstCharacterToEncode);
                        result = new string(wholebuffer, 0, totalWritten);
                    }
                    else
                    {
                        char[] wholebuffer = new char[bufferSize];
                        fixed (char* buffer = &wholebuffer[0])
                        {
                            int totalWritten = EncodeIntoBuffer(buffer, bufferSize, valuePointer, value.Length, firstCharacterToEncode);
                            result = new string(wholebuffer, 0, totalWritten);
                        }
                    }

                    return result;
                }
            }
        }

        // NOTE: The order of the parameters to this method is a work around for https://github.com/dotnet/corefx/issues/4455
        // and the underlying Mono bug: https://bugzilla.xamarin.com/show_bug.cgi?id=36052.
        // If changing the signature of this method, ensure this issue isn't regressing on Mono.
        private unsafe int EncodeIntoBuffer(char* buffer, int bufferLength, char* value, int valueLength, int firstCharacterToEncode)
        {
            int totalWritten = 0;

            if (firstCharacterToEncode > 0)
            {
                int bytesToCopy = firstCharacterToEncode + firstCharacterToEncode;
                BufferInternal.MemoryCopy(value, buffer, bytesToCopy, bytesToCopy);
                totalWritten += firstCharacterToEncode;
                bufferLength -= firstCharacterToEncode;
                buffer += firstCharacterToEncode;
            }

            int valueIndex = firstCharacterToEncode;

            char firstChar = value[valueIndex];
            char secondChar = firstChar;
            bool wasSurrogatePair = false;
            int charsWritten;

            // this loop processes character pairs (in case they are surrogates).
            // there is an if block below to process single last character.
            for (int secondCharIndex = valueIndex + 1; secondCharIndex < valueLength; secondCharIndex++)
            {
                if (!wasSurrogatePair)
                {
                    firstChar = secondChar;
                }
                else
                {
                    firstChar = value[secondCharIndex - 1];
                }
                secondChar = value[secondCharIndex];

                if (!WillEncode(firstChar))
                {
                    wasSurrogatePair = false;
                    *buffer = firstChar;
                    buffer++;
                    bufferLength--;
                    totalWritten++;
                }
                else
                {
                    int nextScalar = UnicodeHelpers.GetScalarValueFromUtf16(firstChar, secondChar, out wasSurrogatePair);
                    if (!TryEncodeUnicodeScalar(nextScalar, buffer, bufferLength, out charsWritten))
                    {
                        throw new ArgumentException("Argument encoder does not implement MaxOutputCharsPerInputChar correctly.");
                    }

                    buffer += charsWritten;
                    bufferLength -= charsWritten;
                    totalWritten += charsWritten;
                    if (wasSurrogatePair)
                    {
                        secondCharIndex++;
                    }
                }
            }

            if (!wasSurrogatePair)
            {
                firstChar = value[valueLength - 1];
                int nextScalar = UnicodeHelpers.GetScalarValueFromUtf16(firstChar, null, out wasSurrogatePair);
                if (!TryEncodeUnicodeScalar(nextScalar, buffer, bufferLength, out charsWritten))
                {
                    throw new ArgumentException("Argument encoder does not implement MaxOutputCharsPerInputChar correctly.");
                }

                buffer += charsWritten;
                bufferLength -= charsWritten;
                totalWritten += charsWritten;
            }

            return totalWritten;
        }

        /// <summary>
        /// Encodes the supplied string into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="output">Encoded text is written to this output.</param>
        /// <param name="value">String to be encoded.</param>
        public void Encode(TextWriter output, string value)
        {
            Encode(output, value, 0, value.Length);
        }

        /// <summary>
        ///  Encodes a substring into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="output">Encoded text is written to this output.</param>
        /// <param name="value">String whose substring is to be encoded.</param>
        /// <param name="startIndex">The index where the substring starts.</param>
        /// <param name="characterCount">Number of characters in the substring.</param>
        public virtual void Encode(TextWriter output, string value, int startIndex, int characterCount)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            ValidateRanges(startIndex, characterCount, actualInputLength: value.Length);

            unsafe
            {
                fixed (char* valuePointer = value)
                {
                    char* substring = valuePointer + startIndex;
                    int firstIndexToEncode = FindFirstCharacterToEncode(substring, characterCount);

                    if (firstIndexToEncode == -1) // nothing to encode; 
                    {
                        if (startIndex == 0 && characterCount == value.Length) // write whole string
                        {
                            output.Write(value);
                            return;
                        }
                        for (int i = 0; i < characterCount; i++) // write substring
                        {
                            output.Write(*substring);
                            substring++;
                        }
                        return;
                    }

                    // write prefix, then encode
                    for (int i = 0; i < firstIndexToEncode; i++)
                    {
                        output.Write(*substring);
                        substring++;
                    }

                    EncodeCore(output, substring, characterCount - firstIndexToEncode);
                }
            }
        }

        /// <summary>
        ///  Encodes characters from an array into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="output">Encoded text is written to the output.</param>
        /// <param name="value">Array of characters to be encoded.</param>
        /// <param name="startIndex">The index where the substring starts.</param>
        /// <param name="characterCount">Number of characters in the substring.</param>
        public virtual void Encode(TextWriter output, char[] value, int startIndex, int characterCount)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            ValidateRanges(startIndex, characterCount, actualInputLength: value.Length);

            unsafe
            {
                fixed (char* valuePointer = value)
                {
                    char* substring = valuePointer + startIndex;
                    int firstIndexToEncode = FindFirstCharacterToEncode(substring, characterCount);

                    if (firstIndexToEncode == -1) // nothing to encode; 
                    {
                        if (startIndex == 0 && characterCount == value.Length) // write whole string
                        {
                            output.Write(value);
                            return;
                        }
                        for (int i = 0; i < characterCount; i++) // write substring
                        {
                            output.Write(*substring);
                            substring++;
                        }
                        return;
                    }

                    // write prefix, then encode
                    for (int i = 0; i < firstIndexToEncode; i++)
                    {
                        output.Write(*substring);
                        substring++;
                    }

                    EncodeCore(output, substring, characterCount - firstIndexToEncode);
                }
            }
        }

        private unsafe void EncodeCore(TextWriter output, char* value, int valueLength)
        {
            Debug.Assert(value != null & output != null);
            Debug.Assert(valueLength >= 0);

            int bufferLength = MaxOutputCharactersPerInputCharacter;
            char* buffer = stackalloc char[bufferLength];

            char firstChar = *value;
            char secondChar = firstChar;
            bool wasSurrogatePair = false;
            int charsWritten;

            // this loop processes character pairs (in case they are surrogates).
            // there is an if block below to process single last character.
            for (int secondCharIndex = 1; secondCharIndex < valueLength; secondCharIndex++)
            {
                if (!wasSurrogatePair)
                {
                    firstChar = secondChar;
                }
                else
                {
                    firstChar = value[secondCharIndex - 1];
                }
                secondChar = value[secondCharIndex];

                if (!WillEncode(firstChar))
                {
                    wasSurrogatePair = false;
                    output.Write(firstChar);
                }
                else
                {
                    int nextScalar = UnicodeHelpers.GetScalarValueFromUtf16(firstChar, secondChar, out wasSurrogatePair);
                    if (!TryEncodeUnicodeScalar(nextScalar, buffer, bufferLength, out charsWritten))
                    {
                        throw new ArgumentException("Argument encoder does not implement MaxOutputCharsPerInputChar correctly.");
                    }
                    Write(output, buffer, charsWritten);

                    if (wasSurrogatePair)
                    {
                        secondCharIndex++;
                    }
                }
            }

            if (!wasSurrogatePair)
            {
                firstChar = value[valueLength - 1];
                int nextScalar = UnicodeHelpers.GetScalarValueFromUtf16(firstChar, null, out wasSurrogatePair);
                if (!TryEncodeUnicodeScalar(nextScalar, buffer, bufferLength, out charsWritten))
                {
                    throw new ArgumentException("Argument encoder does not implement MaxOutputCharsPerInputChar correctly.");
                }
                Write(output, buffer, charsWritten);
            }
        }

        internal static unsafe bool TryCopyCharacters(char[] source, char* destination, int destinationLength, out int numberOfCharactersWritten)
        {
            Debug.Assert(source != null && destination != null && destinationLength >= 0);

            if (destinationLength < source.Length)
            {
                numberOfCharactersWritten = 0;
                return false;
            }

            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i];
            }

            numberOfCharactersWritten = source.Length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool TryWriteScalarAsChar(int unicodeScalar, char* destination, int destinationLength, out int numberOfCharactersWritten)
        {
            Debug.Assert(destination != null && destinationLength >= 0);

            Debug.Assert(unicodeScalar < ushort.MaxValue);
            if (destinationLength < 1)
            {
                numberOfCharactersWritten = 0;
                return false;
            }
            *destination = (char)unicodeScalar;
            numberOfCharactersWritten = 1;
            return true;
        }

        private static void ValidateRanges(int startIndex, int characterCount, int actualInputLength)
        {
            if (startIndex < 0 || startIndex > actualInputLength)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (characterCount < 0 || characterCount > (actualInputLength - startIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(characterCount));
            }
        }

        private static unsafe void Write(TextWriter output, char* input, int inputLength)
        {
            Debug.Assert(output != null && input != null && inputLength >= 0);

            while (inputLength-- > 0)
            {
                output.Write(*input);
                input++;
            }
        }
    }
    internal struct AllowedCharactersBitmap
    {
        private const int ALLOWED_CHARS_BITMAP_LENGTH = 0x10000 / (8 * sizeof(uint));
        private readonly uint[] _allowedCharacters;

        // should be called in place of the default ctor
        public static AllowedCharactersBitmap CreateNew()
        {
            return new AllowedCharactersBitmap(new uint[ALLOWED_CHARS_BITMAP_LENGTH]);
        }

        private AllowedCharactersBitmap(uint[] allowedCharacters)
        {
            if (allowedCharacters == null)
            {
                throw new ArgumentNullException(nameof(allowedCharacters));
            }
            _allowedCharacters = allowedCharacters;
        }

        // Marks a character as allowed (can be returned unencoded)
        public void AllowCharacter(char character)
        {
            int codePoint = character;
            int index = codePoint >> 5;
            int offset = codePoint & 0x1F;
            _allowedCharacters[index] |= 0x1U << offset;
        }

        // Marks a character as forbidden (must be returned encoded)
        public void ForbidCharacter(char character)
        {
            int codePoint = character;
            int index = codePoint >> 5;
            int offset = codePoint & 0x1F;
            _allowedCharacters[index] &= ~(0x1U << offset);
        }

        // Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
        // (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp)
        public void ForbidUndefinedCharacters()
        {
            uint[] definedCharactersBitmap = UnicodeHelpers.GetDefinedCharacterBitmap();
            Debug.Assert(definedCharactersBitmap.Length == _allowedCharacters.Length);
            for (int i = 0; i < _allowedCharacters.Length; i++)
            {
                _allowedCharacters[i] &= definedCharactersBitmap[i];
            }
        }

        // Marks all characters as forbidden (must be returned encoded)
        public void Clear()
        {
            Array.Clear(_allowedCharacters, 0, _allowedCharacters.Length);
        }

        // Creates a deep copy of this bitmap
        public AllowedCharactersBitmap Clone()
        {
            return new AllowedCharactersBitmap((uint[])_allowedCharacters.Clone());
        }

        // Determines whether the given character can be returned unencoded.
        public bool IsCharacterAllowed(char character)
        {
            int codePoint = character;
            int index = codePoint >> 5;
            int offset = codePoint & 0x1F;
            return ((_allowedCharacters[index] >> offset) & 0x1U) != 0;
        }

        // Determines whether the given character can be returned unencoded.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnicodeScalarAllowed(int unicodeScalar)
        {
            int index = unicodeScalar >> 5;
            int offset = unicodeScalar & 0x1F;
            return ((_allowedCharacters[index] >> offset) & 0x1U) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            for (int i = 0; i < textLength; i++)
            {
                if (!IsCharacterAllowed(text[i])) { return i; }
            }
            return -1;
        }
    }
    /// <summary>
    /// Represents a filter which allows only certain Unicode code points through.
    /// </summary>
    public class TextEncoderSettings
    {
        private AllowedCharactersBitmap _allowedCharactersBitmap;

        /// <summary>
        /// Instantiates an empty filter (allows no code points through by default).
        /// </summary>
        public TextEncoderSettings()
        {
            _allowedCharactersBitmap = AllowedCharactersBitmap.CreateNew();
        }

        /// <summary>
        /// Instantiates the filter by cloning the allow list of another <see cref="TextEncoderSettings"/>.
        /// </summary>
        public TextEncoderSettings(TextEncoderSettings other)
        {
            _allowedCharactersBitmap = AllowedCharactersBitmap.CreateNew();
            AllowCodePoints(other.GetAllowedCodePoints());
        }

        /// <summary>
        /// Instantiates the filter where only the character ranges specified by <paramref name="allowedRanges"/>
        /// are allowed by the filter.
        /// </summary>
        public TextEncoderSettings(params UnicodeRange[] allowedRanges)
        {
            if (allowedRanges == null)
            {
                throw new ArgumentNullException(nameof(allowedRanges));
            }
            _allowedCharactersBitmap = AllowedCharactersBitmap.CreateNew();
            AllowRanges(allowedRanges);
        }

        /// <summary>
        /// Allows the character specified by <paramref name="character"/> through the filter.
        /// </summary>
        public virtual void AllowCharacter(char character)
        {
            _allowedCharactersBitmap.AllowCharacter(character);
        }

        /// <summary>
        /// Allows all characters specified by <paramref name="characters"/> through the filter.
        /// </summary>
        public virtual void AllowCharacters(params char[] characters)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            for (int i = 0; i < characters.Length; i++)
            {
                _allowedCharactersBitmap.AllowCharacter(characters[i]);
            }
        }

        /// <summary>
        /// Allows all code points specified by <paramref name="codePoints"/>.
        /// </summary>
        public virtual void AllowCodePoints(IEnumerable<int> codePoints)
        {
            if (codePoints == null)
            {
                throw new ArgumentNullException(nameof(codePoints));
            }

            foreach (var allowedCodePoint in codePoints)
            {
                // If the code point can't be represented as a BMP character, skip it.
                char codePointAsChar = (char)allowedCodePoint;
                if (allowedCodePoint == codePointAsChar)
                {
                    _allowedCharactersBitmap.AllowCharacter(codePointAsChar);
                }
            }
        }

        /// <summary>
        /// Allows all characters specified by <paramref name="range"/> through the filter.
        /// </summary>
        public virtual void AllowRange(UnicodeRange range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            int firstCodePoint = range.FirstCodePoint;
            int rangeSize = range.Length;
            for (int i = 0; i < rangeSize; i++)
            {
                _allowedCharactersBitmap.AllowCharacter((char)(firstCodePoint + i));
            }
        }

        /// <summary>
        /// Allows all characters specified by <paramref name="ranges"/> through the filter.
        /// </summary>
        public virtual void AllowRanges(params UnicodeRange[] ranges)
        {
            if (ranges == null)
            {
                throw new ArgumentNullException(nameof(ranges));
            }

            for (int i = 0; i < ranges.Length; i++)
            {
                AllowRange(ranges[i]);
            }
        }

        /// <summary>
        /// Resets this settings object by disallowing all characters.
        /// </summary>
        public virtual void Clear()
        {
            _allowedCharactersBitmap.Clear();
        }

        /// <summary>
        /// Disallows the character <paramref name="character"/> through the filter.
        /// </summary>
        public virtual void ForbidCharacter(char character)
        {
            _allowedCharactersBitmap.ForbidCharacter(character);
        }

        /// <summary>
        /// Disallows all characters specified by <paramref name="characters"/> through the filter.
        /// </summary>
        public virtual void ForbidCharacters(params char[] characters)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            for (int i = 0; i < characters.Length; i++)
            {
                _allowedCharactersBitmap.ForbidCharacter(characters[i]);
            }
        }

        /// <summary>
        /// Disallows all characters specified by <paramref name="range"/> through the filter.
        /// </summary>
        public virtual void ForbidRange(UnicodeRange range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            int firstCodePoint = range.FirstCodePoint;
            int rangeSize = range.Length;
            for (int i = 0; i < rangeSize; i++)
            {
                _allowedCharactersBitmap.ForbidCharacter((char)(firstCodePoint + i));
            }
        }

        /// <summary>
        /// Disallows all characters specified by <paramref name="ranges"/> through the filter.
        /// </summary>
        public virtual void ForbidRanges(params UnicodeRange[] ranges)
        {
            if (ranges == null)
            {
                throw new ArgumentNullException(nameof(ranges));
            }

            for (int i = 0; i < ranges.Length; i++)
            {
                ForbidRange(ranges[i]);
            }
        }

        /// <summary>
        /// Retrieves the bitmap of allowed characters from this settings object.
        /// The returned bitmap is a clone of the original bitmap to avoid unintentional modification.
        /// </summary>
        internal AllowedCharactersBitmap GetAllowedCharacters()
        {
            return _allowedCharactersBitmap.Clone();
        }

        /// <summary>
        /// Gets an enumeration of all allowed code points.
        /// </summary>
        public virtual IEnumerable<int> GetAllowedCodePoints()
        {
            for (int i = 0; i < 0x10000; i++)
            {
                if (_allowedCharactersBitmap.IsCharacterAllowed((char)i))
                {
                    yield return i;
                }
            }
        }
    }
    /// <summary>
    /// Represents a type used to do HTML encoding.
    /// </summary>
    public abstract class HtmlEncoder : TextEncoder
    {
        /// <summary>
        /// Returns a default built-in instance of <see cref="HtmlEncoder"/>.
        /// </summary>
        public static HtmlEncoder Default
        {
            get { return DefaultHtmlEncoder.Singleton; }
        }

        /// <summary>
        /// Creates a new instance of HtmlEncoder with provided settings.
        /// </summary>
        /// <param name="settings">Settings used to control how the created <see cref="HtmlEncoder"/> encodes, primarily which characters to encode.</param>
        /// <returns>A new instance of the <see cref="HtmlEncoder"/>.</returns>
        public static HtmlEncoder Create(TextEncoderSettings settings)
        {
            return new DefaultHtmlEncoder(settings);
        }

        /// <summary>
        /// Creates a new instance of HtmlEncoder specifying character to be encoded.
        /// </summary>
        /// <param name="allowedRanges">Set of characters that the encoder is allowed to not encode.</param>
        /// <returns>A new instance of the <see cref="HtmlEncoder"/></returns>
        /// <remarks>Some characters in <paramref name="allowedRanges"/> might still get encoded, i.e. this parameter is just telling the encoder what ranges it is allowed to not encode, not what characters it must not encode.</remarks> 
        public static HtmlEncoder Create(params UnicodeRange[] allowedRanges)
        {
            return new DefaultHtmlEncoder(allowedRanges);
        }
    }
    /// <summary>
    /// Represents a type used to do JavaScript encoding/escaping.
    /// </summary>
    public abstract class JavaScriptEncoder : TextEncoder
    {
        /// <summary>
        /// Returns a default built-in instance of <see cref="JavaScriptEncoder"/>.
        /// </summary>
        public static JavaScriptEncoder Default
        {
            get { return DefaultJavaScriptEncoder.Singleton; }
        }

        /// <summary>
        /// Creates a new instance of JavaScriptEncoder with provided settings.
        /// </summary>
        /// <param name="settings">Settings used to control how the created <see cref="JavaScriptEncoder"/> encodes, primarily which characters to encode.</param>
        /// <returns>A new instance of the <see cref="JavaScriptEncoder"/>.</returns>
        public static JavaScriptEncoder Create(TextEncoderSettings settings)
        {
            return new DefaultJavaScriptEncoder(settings);
        }

        /// <summary>
        /// Creates a new instance of JavaScriptEncoder specifying character to be encoded.
        /// </summary>
        /// <param name="allowedRanges">Set of characters that the encoder is allowed to not encode.</param>
        /// <returns>A new instance of the <see cref="JavaScriptEncoder"/>.</returns>
        /// <remarks>Some characters in <paramref name="allowedRanges"/> might still get encoded, i.e. this parameter is just telling the encoder what ranges it is allowed to not encode, not what characters it must not encode.</remarks> 
        public static JavaScriptEncoder Create(params UnicodeRange[] allowedRanges)
        {
            return new DefaultJavaScriptEncoder(allowedRanges);
        }
    }
    /// <summary>
    /// Represents a type used to do URL encoding.
    /// </summary>
    public abstract class UrlEncoder : TextEncoder
    {
        /// <summary>
        /// Returns a default built-in instance of <see cref="UrlEncoder"/>.
        /// </summary>
        public static UrlEncoder Default
        {
            get { return DefaultUrlEncoder.Singleton; }
        }

        /// <summary>
        /// Creates a new instance of UrlEncoder with provided settings.
        /// </summary>
        /// <param name="settings">Settings used to control how the created <see cref="UrlEncoder"/> encodes, primarily which characters to encode.</param>
        /// <returns>A new instance of the <see cref="UrlEncoder"/>.</returns>
        public static UrlEncoder Create(TextEncoderSettings settings)
        {
            return new DefaultUrlEncoder(settings);
        }

        /// <summary>
        /// Creates a new instance of UrlEncoder specifying character to be encoded.
        /// </summary>
        /// <param name="allowedRanges">Set of characters that the encoder is allowed to not encode.</param>
        /// <returns>A new instance of the <see cref="UrlEncoder"/>.</returns>
        /// <remarks>Some characters in <paramref name="allowedRanges"/> might still get encoded, i.e. this parameter is just telling the encoder what ranges it is allowed to not encode, not what characters it must not encode.</remarks> 
        public static UrlEncoder Create(params UnicodeRange[] allowedRanges)
        {
            return new DefaultUrlEncoder(allowedRanges);
        }
    }

    internal sealed class DefaultUrlEncoder : UrlEncoder
    {
        private AllowedCharactersBitmap _allowedCharacters;

        internal static readonly DefaultUrlEncoder Singleton = new DefaultUrlEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

        // We perform UTF8 conversion of input, which means that the worst case is
        // 12 output chars per input surrogate char: [input] U+FFFF U+FFFF -> [output] "%XX%YY%ZZ%WW".
        public override int MaxOutputCharactersPerInputCharacter
        {
            get { return 12; }
        }

        public DefaultUrlEncoder(TextEncoderSettings filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _allowedCharacters = filter.GetAllowedCharacters();

            // Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
            // (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp)
            _allowedCharacters.ForbidUndefinedCharacters();

            // Forbid characters that are special in HTML.
            // Even though this is a not HTML encoder, 
            // it's unfortunately common for developers to
            // forget to HTML-encode a string once it has been URL-encoded,
            // so this offers extra protection.
            DefaultHtmlEncoder.ForbidHtmlCharacters(_allowedCharacters);

            // Per RFC 3987, Sec. 2.2, we want encodings that are safe for
            // four particular components: 'isegment', 'ipath-noscheme',
            // 'iquery', and 'ifragment'. The relevant definitions are below.
            //
            //    ipath-noscheme = isegment-nz-nc *( "/" isegment )
            // 
            //    isegment       = *ipchar
            // 
            //    isegment-nz-nc = 1*( iunreserved / pct-encoded / sub-delims
            //                         / "@" )
            //                   ; non-zero-length segment without any colon ":"
            //
            //    ipchar         = iunreserved / pct-encoded / sub-delims / ":"
            //                   / "@"
            // 
            //    iquery         = *( ipchar / iprivate / "/" / "?" )
            // 
            //    ifragment      = *( ipchar / "/" / "?" )
            // 
            //    iunreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
            // 
            //    ucschar        = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF
            //                   / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD
            //                   / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD
            //                   / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD
            //                   / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD
            //                   / %xD0000-DFFFD / %xE1000-EFFFD
            // 
            //    pct-encoded    = "%" HEXDIG HEXDIG
            // 
            //    sub-delims     = "!" / "$" / "&" / "'" / "(" / ")"
            //                   / "*" / "+" / "," / ";" / "="
            //
            // The only common characters between these four components are the
            // intersection of 'isegment-nz-nc' and 'ipchar', which is really
            // just 'isegment-nz-nc' (colons forbidden).
            // 
            // From this list, the base encoder already forbids "&", "'", "+",
            // and we'll additionally forbid "=" since it has special meaning
            // in x-www-form-urlencoded representations.
            //
            // This means that the full list of allowed characters from the
            // Basic Latin set is:
            // ALPHA / DIGIT / "-" / "." / "_" / "~" / "!" / "$" / "(" / ")" / "*" / "," / ";" / "@"

            const string forbiddenChars = @" #%/:=?[\]^`{|}"; // chars from Basic Latin which aren't already disallowed by the base encoder
            foreach (char character in forbiddenChars)
            {
                _allowedCharacters.ForbidCharacter(character);
            }

            // Specials (U+FFF0 .. U+FFFF) are forbidden by the definition of 'ucschar' above
            for (int i = 0; i < 16; i++)
            {
                _allowedCharacters.ForbidCharacter((char)(0xFFF0 | i));
            }
        }

        public DefaultUrlEncoder(params UnicodeRange[] allowedRanges) : this(new TextEncoderSettings(allowedRanges))
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool WillEncode(int unicodeScalar)
        {
            if (UnicodeHelpers.IsSupplementaryCodePoint(unicodeScalar)) return true;
            return !_allowedCharacters.IsUnicodeScalarAllowed(unicodeScalar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return _allowedCharacters.FindFirstCharacterToEncode(text, textLength);
        }

        public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (!WillEncode(unicodeScalar)) { return TryWriteScalarAsChar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten); }

            numberOfCharactersWritten = 0;
            uint asUtf8 = unchecked((uint)UnicodeHelpers.GetUtf8RepresentationForScalarValue((uint)unicodeScalar));
            do
            {
                char highNibble, lowNibble;
                HexUtil.ByteToHexDigits(unchecked((byte)asUtf8), out highNibble, out lowNibble);

                if (numberOfCharactersWritten + 3 > bufferLength)
                {
                    numberOfCharactersWritten = 0;
                    return false;
                }

                *buffer = '%'; buffer++;
                *buffer = highNibble; buffer++;
                *buffer = lowNibble; buffer++;

                numberOfCharactersWritten += 3;
            }
            while ((asUtf8 >>= 8) != 0);
            return true;
        }
    }
    internal sealed class DefaultJavaScriptEncoder : JavaScriptEncoder
    {
        private AllowedCharactersBitmap _allowedCharacters;

        internal static readonly DefaultJavaScriptEncoder Singleton = new DefaultJavaScriptEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

        public DefaultJavaScriptEncoder(TextEncoderSettings filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _allowedCharacters = filter.GetAllowedCharacters();

            // Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
            // (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp)
            _allowedCharacters.ForbidUndefinedCharacters();

            // Forbid characters that are special in HTML.
            // Even though this is a not HTML encoder, 
            // it's unfortunately common for developers to
            // forget to HTML-encode a string once it has been JS-encoded,
            // so this offers extra protection.
            DefaultHtmlEncoder.ForbidHtmlCharacters(_allowedCharacters);

            _allowedCharacters.ForbidCharacter('\\');
            _allowedCharacters.ForbidCharacter('/');

            // Forbid GRAVE ACCENT \u0060 character.
            _allowedCharacters.ForbidCharacter('`');
        }

        public DefaultJavaScriptEncoder(params UnicodeRange[] allowedRanges) : this(new TextEncoderSettings(allowedRanges))
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool WillEncode(int unicodeScalar)
        {
            if (UnicodeHelpers.IsSupplementaryCodePoint(unicodeScalar)) return true;
            return !_allowedCharacters.IsUnicodeScalarAllowed(unicodeScalar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return _allowedCharacters.FindFirstCharacterToEncode(text, textLength);
        }

        // The worst case encoding is 6 output chars per input char: [input] U+FFFF -> [output] "\uFFFF"
        // We don't need to worry about astral code points since they're represented as encoded
        // surrogate pairs in the output.
        public override int MaxOutputCharactersPerInputCharacter
        {
            get { return 12; } // "\uFFFF\uFFFF" is the longest encoded form 
        }

        static readonly char[] s_b = new char[] { '\\', 'b' };
        static readonly char[] s_t = new char[] { '\\', 't' };
        static readonly char[] s_n = new char[] { '\\', 'n' };
        static readonly char[] s_f = new char[] { '\\', 'f' };
        static readonly char[] s_r = new char[] { '\\', 'r' };
        static readonly char[] s_forward = new char[] {  '/' };//'\\',
        static readonly char[] s_back = new char[] { '\\', '\\' };

        // Writes a scalar value as a JavaScript-escaped character (or sequence of characters).
        // See ECMA-262, Sec. 7.8.4, and ECMA-404, Sec. 9
        // http://www.ecma-international.org/ecma-262/5.1/#sec-7.8.4
        // http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-404.pdf
        public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            // ECMA-262 allows encoding U+000B as "\v", but ECMA-404 does not.
            // Both ECMA-262 and ECMA-404 allow encoding U+002F SOLIDUS as "\/".
            // (In ECMA-262 this character is a NonEscape character.)
            // HTML-specific characters (including apostrophe and quotes) will
            // be written out as numeric entities for defense-in-depth.
            // See UnicodeEncoderBase ctor comments for more info.

            if (!WillEncode(unicodeScalar)) { return TryWriteScalarAsChar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten); }

            char[] toCopy = null;
            switch (unicodeScalar)
            {
                case '\b': toCopy = s_b; break;
                case '\t': toCopy = s_t; break;
                case '\n': toCopy = s_n; break;
                case '\f': toCopy = s_f; break;
                case '\r': toCopy = s_r; break;
                case '/': toCopy = s_forward; break;
                case '\\': toCopy = s_back; break;
                default: return TryWriteEncodedScalarAsNumericEntity(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
            }
            return TryCopyCharacters(toCopy, buffer, bufferLength, out numberOfCharactersWritten);
        }

        private static unsafe bool TryWriteEncodedScalarAsNumericEntity(int unicodeScalar, char* buffer, int length, out int numberOfCharactersWritten)
        {
            Debug.Assert(buffer != null && length >= 0);

            if (UnicodeHelpers.IsSupplementaryCodePoint(unicodeScalar))
            {
                // Convert this back to UTF-16 and write out both characters.
                char leadingSurrogate, trailingSurrogate;
                UnicodeHelpers.GetUtf16SurrogatePairFromAstralScalarValue(unicodeScalar, out leadingSurrogate, out trailingSurrogate);
                int leadingSurrogateCharactersWritten;
                if (TryWriteEncodedSingleCharacter(leadingSurrogate, buffer, length, out leadingSurrogateCharactersWritten) &&
                    TryWriteEncodedSingleCharacter(trailingSurrogate, buffer + leadingSurrogateCharactersWritten, length - leadingSurrogateCharactersWritten, out numberOfCharactersWritten)
                )
                {
                    numberOfCharactersWritten += leadingSurrogateCharactersWritten;
                    return true;
                }
                else
                {
                    numberOfCharactersWritten = 0;
                    return false;
                }
            }
            else
            {
                // This is only a single character.
                return TryWriteEncodedSingleCharacter(unicodeScalar, buffer, length, out numberOfCharactersWritten);
            }
        }

        // Writes an encoded scalar value (in the BMP) as a JavaScript-escaped character.
        private static unsafe bool TryWriteEncodedSingleCharacter(int unicodeScalar, char* buffer, int length, out int numberOfCharactersWritten)
        {
            Debug.Assert(buffer != null && length >= 0);
            Debug.Assert(!UnicodeHelpers.IsSupplementaryCodePoint(unicodeScalar), "The incoming value should've been in the BMP.");

            if (length < 6)
            {
                numberOfCharactersWritten = 0;
                return false;
            }

            // Encode this as 6 chars "\uFFFF".
            *buffer = '\\'; buffer++;
            *buffer = 'u'; buffer++;
            *buffer = HexUtil.Int32LsbToHexDigit(unicodeScalar >> 12); buffer++;
            *buffer = HexUtil.Int32LsbToHexDigit((int)((unicodeScalar >> 8) & 0xFU)); buffer++;
            *buffer = HexUtil.Int32LsbToHexDigit((int)((unicodeScalar >> 4) & 0xFU)); buffer++;
            *buffer = HexUtil.Int32LsbToHexDigit((int)(unicodeScalar & 0xFU)); buffer++;

            numberOfCharactersWritten = 6;
            return true;
        }
    }
    internal sealed class DefaultHtmlEncoder : HtmlEncoder
    {
        private AllowedCharactersBitmap _allowedCharacters;
        internal static readonly DefaultHtmlEncoder Singleton = new DefaultHtmlEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

        public DefaultHtmlEncoder(TextEncoderSettings filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _allowedCharacters = filter.GetAllowedCharacters();

            // Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
            // (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp)
            _allowedCharacters.ForbidUndefinedCharacters();

            ForbidHtmlCharacters(_allowedCharacters);
        }

        internal static void ForbidHtmlCharacters(AllowedCharactersBitmap allowedCharacters)
        {
            allowedCharacters.ForbidCharacter('<');
            allowedCharacters.ForbidCharacter('>');
            allowedCharacters.ForbidCharacter('&');
            allowedCharacters.ForbidCharacter('\''); // can be used to escape attributes
            allowedCharacters.ForbidCharacter('\"'); // can be used to escape attributes
            allowedCharacters.ForbidCharacter('+'); // technically not HTML-specific, but can be used to perform UTF7-based attacks
        }

        public DefaultHtmlEncoder(params UnicodeRange[] allowedRanges) : this(new TextEncoderSettings(allowedRanges))
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool WillEncode(int unicodeScalar)
        {
            if (UnicodeHelpers.IsSupplementaryCodePoint(unicodeScalar)) return true;
            return !_allowedCharacters.IsUnicodeScalarAllowed(unicodeScalar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
        {
            return _allowedCharacters.FindFirstCharacterToEncode(text, textLength);
        }

        public override int MaxOutputCharactersPerInputCharacter
        {
            get { return 10; } // "&#x10FFFF;" is the longest encoded form
        }

        static readonly char[] s_quote = "&quot;".ToCharArray();
        static readonly char[] s_ampersand = "&amp;".ToCharArray();
        static readonly char[] s_lessthan = "&lt;".ToCharArray();
        static readonly char[] s_greaterthan = "&gt;".ToCharArray();

        public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (!WillEncode(unicodeScalar)) { return TryWriteScalarAsChar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten); }
            else if (unicodeScalar == '\"') { return TryCopyCharacters(s_quote, buffer, bufferLength, out numberOfCharactersWritten); }
            else if (unicodeScalar == '&') { return TryCopyCharacters(s_ampersand, buffer, bufferLength, out numberOfCharactersWritten); }
            else if (unicodeScalar == '<') { return TryCopyCharacters(s_lessthan, buffer, bufferLength, out numberOfCharactersWritten); }
            else if (unicodeScalar == '>') { return TryCopyCharacters(s_greaterthan, buffer, bufferLength, out numberOfCharactersWritten); }
            else { return TryWriteEncodedScalarAsNumericEntity(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten); }
        }

        private static unsafe bool TryWriteEncodedScalarAsNumericEntity(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            Debug.Assert(buffer != null && bufferLength >= 0);

            // We're writing the characters in reverse, first determine
            // how many there are
            const int nibbleSize = 4;
            int numberOfHexCharacters = 0;
            int compareUnicodeScalar = unicodeScalar;

            do
            {
                Debug.Assert(numberOfHexCharacters < 8, "Couldn't have written 8 characters out by this point.");
                numberOfHexCharacters++;
                compareUnicodeScalar >>= nibbleSize;
            } while (compareUnicodeScalar != 0);

            numberOfCharactersWritten = numberOfHexCharacters + 4; // four chars are &, #, x, and ;
            Debug.Assert(numberOfHexCharacters > 0, "At least one character should've been written.");

            if (numberOfHexCharacters + 4 > bufferLength)
            {
                numberOfCharactersWritten = 0;
                return false;
            }
            // Finally, write out the HTML-encoded scalar value.
            *buffer = '&';
            buffer++;
            *buffer = '#';
            buffer++;
            *buffer = 'x';

            // Jump to the end of the hex position and write backwards
            buffer += numberOfHexCharacters;
            do
            {
                *buffer = HexUtil.Int32LsbToHexDigit(unicodeScalar & 0xF);
                unicodeScalar >>= nibbleSize;
                buffer--;
            }
            while (unicodeScalar != 0);

            buffer += numberOfHexCharacters + 1;
            *buffer = ';';
            return true;
        }
    }
}
