using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace MuPDFCore
{
    /// <summary>
    /// Represents a language used by Tesseract OCR.
    /// </summary>
    public class TesseractLanguage
    {
        /// <summary>
        /// The name of the folder where the language file is located.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The name of the language. The Tesseract library will assume that the trained language data file can be found at <c>Prefix/Language.traineddata</c>.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Fast integer versions of trained models. These are models for a single language.
        /// </summary>
        public enum Fast
        {
            /// <summary>
            /// The Afrikaans language.
            /// </summary>
            Afr,
            /// <summary>
            /// The Amharic language.
            /// </summary>
            Amh,
            /// <summary>
            /// The Arabic language.
            /// </summary>
            Ara,
            /// <summary>
            /// The Assamese language.
            /// </summary>
            Asm,
            /// <summary>
            /// The Azerbaijani language.
            /// </summary>
            Aze,
            /// <summary>
            /// The Azerbaijani language (Cyrillic).
            /// </summary>
            Aze_Cyrl,
            /// <summary>
            /// The Belarusian language.
            /// </summary>
            Bel,
            /// <summary>
            /// The Bengali language.
            /// </summary>
            Ben,
            /// <summary>
            /// The Tibetan language.
            /// </summary>
            Bod,
            /// <summary>
            /// The Bosnian language.
            /// </summary>
            Bos,
            /// <summary>
            /// The Breton language.
            /// </summary>
            Bre,
            /// <summary>
            /// The Bulgarian language.
            /// </summary>
            Bul,
            /// <summary>
            /// The Catalan/Valencian language.
            /// </summary>
            Cat,
            /// <summary>
            /// The Cebuano language.
            /// </summary>
            Ceb,
            /// <summary>
            /// The Czech language.
            /// </summary>
            Ces,
            /// <summary>
            /// The Chinese (Simplified) language.
            /// </summary>
            Chi_Sim,
            /// <summary>
            /// The Chinese (Simplified) language (vertical).
            /// </summary>
            Chi_Sim_Vert,
            /// <summary>
            /// The Chinese (Traditional) language.
            /// </summary>
            Chi_Tra,
            /// <summary>
            /// The Chinese (Traditional) language (vertical).
            /// </summary>
            Chi_Tra_Vert,
            /// <summary>
            /// The Cherokee language.
            /// </summary>
            Chr,
            /// <summary>
            /// The Corsican language.
            /// </summary>
            Cos,
            /// <summary>
            /// The Welsh language.
            /// </summary>
            Cym,
            /// <summary>
            /// The Danish language.
            /// </summary>
            Dan,
            /// <summary>
            /// The German language.
            /// </summary>
            Deu,
            /// <summary>
            /// The Divehi/Dhivehi/Maldivian language.
            /// </summary>
            Div,
            /// <summary>
            /// The Dzongkha language.
            /// </summary>
            Dzo,
            /// <summary>
            /// The Greek, Modern (1453-) language.
            /// </summary>
            Ell,
            /// <summary>
            /// The English language.
            /// </summary>
            Eng,
            /// <summary>
            /// The English, Middle (1100-1500) language.
            /// </summary>
            Enm,
            /// <summary>
            /// The Esperanto language.
            /// </summary>
            Epo,
            /// <summary>
            /// A language for equations.
            /// </summary>
            Equ,
            /// <summary>
            /// The Estonian language.
            /// </summary>
            Est,
            /// <summary>
            /// The Basque language.
            /// </summary>
            Eus,
            /// <summary>
            /// The Faroese language.
            /// </summary>
            Fao,
            /// <summary>
            /// The Persian language.
            /// </summary>
            Fas,
            /// <summary>
            /// The Filipino/Pilipino language.
            /// </summary>
            Fil,
            /// <summary>
            /// The Finnish language.
            /// </summary>
            Fin,
            /// <summary>
            /// The French language.
            /// </summary>
            Fra,
            /// <summary>
            /// The German - Fraktur language.
            /// </summary>
            Frk,
            /// <summary>
            /// The French, Middle (ca.1400-1600) language.
            /// </summary>
            Frm,
            /// <summary>
            /// The Western Frisian language.
            /// </summary>
            Fry,
            /// <summary>
            /// The Gaelic/Scottish Gaelic language.
            /// </summary>
            Gla,
            /// <summary>
            /// The Irish language.
            /// </summary>
            Gle,
            /// <summary>
            /// The Galician language.
            /// </summary>
            Glg,
            /// <summary>
            /// The Greek, Ancient (to 1453) language.
            /// </summary>
            Grc,
            /// <summary>
            /// The Gujarati language.
            /// </summary>
            Guj,
            /// <summary>
            /// The Haitian/Haitian Creole language.
            /// </summary>
            Hat,
            /// <summary>
            /// The Hebrew language.
            /// </summary>
            Heb,
            /// <summary>
            /// The Hindi language.
            /// </summary>
            Hin,
            /// <summary>
            /// The Croatian language.
            /// </summary>
            Hrv,
            /// <summary>
            /// The Hungarian language.
            /// </summary>
            Hun,
            /// <summary>
            /// The Armenian language.
            /// </summary>
            Hye,
            /// <summary>
            /// The Inuktitut language.
            /// </summary>
            Iku,
            /// <summary>
            /// The Indonesian language.
            /// </summary>
            Ind,
            /// <summary>
            /// The Icelandic language.
            /// </summary>
            Isl,
            /// <summary>
            /// The Italian language.
            /// </summary>
            Ita,
            /// <summary>
            /// The Italian language (old).
            /// </summary>
            Ita_Old,
            /// <summary>
            /// The Javanese language.
            /// </summary>
            Jav,
            /// <summary>
            /// The Japanese language.
            /// </summary>
            Jpn,
            /// <summary>
            /// The Japanese language (vertical).
            /// </summary>
            Jpn_Vert,
            /// <summary>
            /// The Kannada language.
            /// </summary>
            Kan,
            /// <summary>
            /// The Georgian language.
            /// </summary>
            Kat,
            /// <summary>
            /// The Georgian language (old).
            /// </summary>
            Kat_Old,
            /// <summary>
            /// The Kazakh language.
            /// </summary>
            Kaz,
            /// <summary>
            /// The Central Khmer language.
            /// </summary>
            Khm,
            /// <summary>
            /// The Kirghiz/Kyrgyz language.
            /// </summary>
            Kir,
            /// <summary>
            /// The Northern Kurdish language.
            /// </summary>
            Kmr,
            /// <summary>
            /// The Korean language.
            /// </summary>
            Kor,
            /// <summary>
            /// The Korean language (vertical).
            /// </summary>
            Kor_Vert,
            /// <summary>
            /// The Lao language.
            /// </summary>
            Lao,
            /// <summary>
            /// The Latin language.
            /// </summary>
            Lat,
            /// <summary>
            /// The Latvian language.
            /// </summary>
            Lav,
            /// <summary>
            /// The Lithuanian language.
            /// </summary>
            Lit,
            /// <summary>
            /// The Luxembourgish/Letzeburgesch language.
            /// </summary>
            Ltz,
            /// <summary>
            /// The Malayalam language.
            /// </summary>
            Mal,
            /// <summary>
            /// The Marathi language.
            /// </summary>
            Mar,
            /// <summary>
            /// The Macedonian language.
            /// </summary>
            Mkd,
            /// <summary>
            /// The Maltese language.
            /// </summary>
            Mlt,
            /// <summary>
            /// The Mongolian language.
            /// </summary>
            Mon,
            /// <summary>
            /// The Maori language.
            /// </summary>
            Mri,
            /// <summary>
            /// The Malay language.
            /// </summary>
            Msa,
            /// <summary>
            /// The Burmese language.
            /// </summary>
            Mya,
            /// <summary>
            /// The Nepali language.
            /// </summary>
            Nep,
            /// <summary>
            /// The Dutch/Flemish language.
            /// </summary>
            Nld,
            /// <summary>
            /// The Norwegian language.
            /// </summary>
            Nor,
            /// <summary>
            /// The Occitan (post 1500) language.
            /// </summary>
            Oci,
            /// <summary>
            /// The Oriya language.
            /// </summary>
            Ori,
            /// <summary>
            /// The Orientation and script detection module.
            /// </summary>
            Osd,
            /// <summary>
            /// The Panjabi/Punjabi language.
            /// </summary>
            Pan,
            /// <summary>
            /// The Polish language.
            /// </summary>
            Pol,
            /// <summary>
            /// The Portuguese language.
            /// </summary>
            Por,
            /// <summary>
            /// The Pushto/Pashto language.
            /// </summary>
            Pus,
            /// <summary>
            /// The Quechua language.
            /// </summary>
            Que,
            /// <summary>
            /// The Romanian/Moldavian/Moldovan language.
            /// </summary>
            Ron,
            /// <summary>
            /// The Russian language.
            /// </summary>
            Rus,
            /// <summary>
            /// The Sanskrit language.
            /// </summary>
            San,
            /// <summary>
            /// The Sinhala/Sinhalese language.
            /// </summary>
            Sin,
            /// <summary>
            /// The Slovak language.
            /// </summary>
            Slk,
            /// <summary>
            /// The Slovenian language.
            /// </summary>
            Slv,
            /// <summary>
            /// The Sindhi language.
            /// </summary>
            Snd,
            /// <summary>
            /// The Spanish/Castilian language.
            /// </summary>
            Spa,
            /// <summary>
            /// The Spanish/Castilian language (old).
            /// </summary>
            Spa_Old,
            /// <summary>
            /// The Albanian language.
            /// </summary>
            Sqi,
            /// <summary>
            /// The Serbian language.
            /// </summary>
            Srp,
            /// <summary>
            /// The Serbian language (Latin).
            /// </summary>
            Srp_Latn,
            /// <summary>
            /// The Sundanese language.
            /// </summary>
            Sun,
            /// <summary>
            /// The Swahili language.
            /// </summary>
            Swa,
            /// <summary>
            /// The Swedish language.
            /// </summary>
            Swe,
            /// <summary>
            /// The Syriac language.
            /// </summary>
            Syr,
            /// <summary>
            /// The Tamil language.
            /// </summary>
            Tam,
            /// <summary>
            /// The Tatar language.
            /// </summary>
            Tat,
            /// <summary>
            /// The Telugu language.
            /// </summary>
            Tel,
            /// <summary>
            /// The Tajik language.
            /// </summary>
            Tgk,
            /// <summary>
            /// The Thai language.
            /// </summary>
            Tha,
            /// <summary>
            /// The Tigrinya language.
            /// </summary>
            Tir,
            /// <summary>
            /// The Tonga (Tonga Islands) language.
            /// </summary>
            Ton,
            /// <summary>
            /// The Turkish language.
            /// </summary>
            Tur,
            /// <summary>
            /// The Uighur/Uyghur language.
            /// </summary>
            Uig,
            /// <summary>
            /// The Ukrainian language.
            /// </summary>
            Ukr,
            /// <summary>
            /// The Urdu language.
            /// </summary>
            Urd,
            /// <summary>
            /// The Uzbek language.
            /// </summary>
            Uzb,
            /// <summary>
            /// The Uzbek language (Cyrillic).
            /// </summary>
            Uzb_Cyrl,
            /// <summary>
            /// The Vietnamese language.
            /// </summary>
            Vie,
            /// <summary>
            /// The Yiddish language.
            /// </summary>
            Yid,
            /// <summary>
            /// The Yoruba language.
            /// </summary>
            Yor
        }

        /// <summary>
        /// Fast integer versions of trained models. These are models for a single script supporting one or more languages.
        /// </summary>
        public enum FastScripts
        {
            /// <summary>
            /// The Arabic script.
            /// </summary>
            Arabic,
            /// <summary>
            /// The Armenian script.
            /// </summary>
            Armenian,
            /// <summary>
            /// The Bengali script.
            /// </summary>
            Bengali,
            /// <summary>
            /// The Canadian Aboriginal script.
            /// </summary>
            Canadian_Aboriginal,
            /// <summary>
            /// The Cherokee script.
            /// </summary>
            Cherokee,
            /// <summary>
            /// The Cyrillic script.
            /// </summary>
            Cyrillic,
            /// <summary>
            /// The Devanagari script.
            /// </summary>
            Devanagari,
            /// <summary>
            /// The Ethiopic script.
            /// </summary>
            Ethiopic,
            /// <summary>
            /// The Fraktur script.
            /// </summary>
            Fraktur,
            /// <summary>
            /// The Georgian script.
            /// </summary>
            Georgian,
            /// <summary>
            /// The Greek script.
            /// </summary>
            Greek,
            /// <summary>
            /// The Gujarati script.
            /// </summary>
            Gujarati,
            /// <summary>
            /// The Gurmukhi script.
            /// </summary>
            Gurmukhi,
            /// <summary>
            /// The Han (Simplified) script.
            /// </summary>
            HanS,
            /// <summary>
            /// The Han (Simplified) script. (vertical)
            /// </summary>
            HanS_Vert,
            /// <summary>
            /// The Han (Traditional) script.
            /// </summary>
            HanT,
            /// <summary>
            /// The Han (Traditional) script. (vertical)
            /// </summary>
            HanT_Vert,
            /// <summary>
            /// The Hangul script.
            /// </summary>
            Hangul,
            /// <summary>
            /// The Hangul script. (vertical)
            /// </summary>
            Hangul_Vert,
            /// <summary>
            /// The Hebrew script.
            /// </summary>
            Hebrew,
            /// <summary>
            /// The Japanese script.
            /// </summary>
            Japanese,
            /// <summary>
            /// The Japanese script. (vertical)
            /// </summary>
            Japanese_Vert,
            /// <summary>
            /// The Kannada script.
            /// </summary>
            Kannada,
            /// <summary>
            /// The Khmer script.
            /// </summary>
            Khmer,
            /// <summary>
            /// The Lao script.
            /// </summary>
            Lao,
            /// <summary>
            /// The Latin script.
            /// </summary>
            Latin,
            /// <summary>
            /// The Malayalam script.
            /// </summary>
            Malayalam,
            /// <summary>
            /// The Myanmar script.
            /// </summary>
            Myanmar,
            /// <summary>
            /// The Oriya script.
            /// </summary>
            Oriya,
            /// <summary>
            /// The Sinhala script.
            /// </summary>
            Sinhala,
            /// <summary>
            /// The Syriac script.
            /// </summary>
            Syriac,
            /// <summary>
            /// The Tamil script.
            /// </summary>
            Tamil,
            /// <summary>
            /// The Telugu script.
            /// </summary>
            Telugu,
            /// <summary>
            /// The Thaana script.
            /// </summary>
            Thaana,
            /// <summary>
            /// The Thai script.
            /// </summary>
            Thai,
            /// <summary>
            /// The Tibetan script.
            /// </summary>
            Tibetan,
            /// <summary>
            /// The Vietnamese script.
            /// </summary>
            Vietnamese
        }

        /// <summary>
        /// Best (most accurate) trained models. These are models for a single language.
        /// </summary>
        public enum Best
        {
            /// <summary>
            /// The Afrikaans language.
            /// </summary>
            Afr,
            /// <summary>
            /// The Amharic language.
            /// </summary>
            Amh,
            /// <summary>
            /// The Arabic language.
            /// </summary>
            Ara,
            /// <summary>
            /// The Assamese language.
            /// </summary>
            Asm,
            /// <summary>
            /// The Azerbaijani language.
            /// </summary>
            Aze,
            /// <summary>
            /// The Azerbaijani language (Cyrillic).
            /// </summary>
            Aze_Cyrl,
            /// <summary>
            /// The Belarusian language.
            /// </summary>
            Bel,
            /// <summary>
            /// The Bengali language.
            /// </summary>
            Ben,
            /// <summary>
            /// The Tibetan language.
            /// </summary>
            Bod,
            /// <summary>
            /// The Bosnian language.
            /// </summary>
            Bos,
            /// <summary>
            /// The Breton language.
            /// </summary>
            Bre,
            /// <summary>
            /// The Bulgarian language.
            /// </summary>
            Bul,
            /// <summary>
            /// The Catalan/Valencian language.
            /// </summary>
            Cat,
            /// <summary>
            /// The Cebuano language.
            /// </summary>
            Ceb,
            /// <summary>
            /// The Czech language.
            /// </summary>
            Ces,
            /// <summary>
            /// The Chinese (Simplified) language.
            /// </summary>
            Chi_Sim,
            /// <summary>
            /// The Chinese (Simplified) language (vertical).
            /// </summary>
            Chi_Sim_Vert,
            /// <summary>
            /// The Chinese (Traditional) language.
            /// </summary>
            Chi_Tra,
            /// <summary>
            /// The Chinese (Traditional) language (vertical).
            /// </summary>
            Chi_Tra_Vert,
            /// <summary>
            /// The Cherokee language.
            /// </summary>
            Chr,
            /// <summary>
            /// The Corsican language.
            /// </summary>
            Cos,
            /// <summary>
            /// The Welsh language.
            /// </summary>
            Cym,
            /// <summary>
            /// The Danish language.
            /// </summary>
            Dan,
            /// <summary>
            /// The German language.
            /// </summary>
            Deu,
            /// <summary>
            /// The Divehi/Dhivehi/Maldivian language.
            /// </summary>
            Div,
            /// <summary>
            /// The Dzongkha language.
            /// </summary>
            Dzo,
            /// <summary>
            /// The Greek, Modern (1453-) language.
            /// </summary>
            Ell,
            /// <summary>
            /// The English language.
            /// </summary>
            Eng,
            /// <summary>
            /// The English, Middle (1100-1500) language.
            /// </summary>
            Enm,
            /// <summary>
            /// The Esperanto language.
            /// </summary>
            Epo,
            /// <summary>
            /// The Estonian language.
            /// </summary>
            Est,
            /// <summary>
            /// The Basque language.
            /// </summary>
            Eus,
            /// <summary>
            /// The Faroese language.
            /// </summary>
            Fao,
            /// <summary>
            /// The Persian language.
            /// </summary>
            Fas,
            /// <summary>
            /// The Filipino/Pilipino language.
            /// </summary>
            Fil,
            /// <summary>
            /// The Finnish language.
            /// </summary>
            Fin,
            /// <summary>
            /// The French language.
            /// </summary>
            Fra,
            /// <summary>
            /// The German - Fraktur language.
            /// </summary>
            Frk,
            /// <summary>
            /// The French, Middle (ca.1400-1600) language.
            /// </summary>
            Frm,
            /// <summary>
            /// The Western Frisian language.
            /// </summary>
            Fry,
            /// <summary>
            /// The Gaelic/Scottish Gaelic language.
            /// </summary>
            Gla,
            /// <summary>
            /// The Irish language.
            /// </summary>
            Gle,
            /// <summary>
            /// The Galician language.
            /// </summary>
            Glg,
            /// <summary>
            /// The Greek, Ancient (to 1453) language.
            /// </summary>
            Grc,
            /// <summary>
            /// The Gujarati language.
            /// </summary>
            Guj,
            /// <summary>
            /// The Haitian/Haitian Creole language.
            /// </summary>
            Hat,
            /// <summary>
            /// The Hebrew language.
            /// </summary>
            Heb,
            /// <summary>
            /// The Hindi language.
            /// </summary>
            Hin,
            /// <summary>
            /// The Croatian language.
            /// </summary>
            Hrv,
            /// <summary>
            /// The Hungarian language.
            /// </summary>
            Hun,
            /// <summary>
            /// The Armenian language.
            /// </summary>
            Hye,
            /// <summary>
            /// The Inuktitut language.
            /// </summary>
            Iku,
            /// <summary>
            /// The Indonesian language.
            /// </summary>
            Ind,
            /// <summary>
            /// The Icelandic language.
            /// </summary>
            Isl,
            /// <summary>
            /// The Italian language.
            /// </summary>
            Ita,
            /// <summary>
            /// The Italian language (old).
            /// </summary>
            Ita_Old,
            /// <summary>
            /// The Javanese language.
            /// </summary>
            Jav,
            /// <summary>
            /// The Japanese language.
            /// </summary>
            Jpn,
            /// <summary>
            /// The Japanese language (vertical).
            /// </summary>
            Jpn_Vert,
            /// <summary>
            /// The Kannada language.
            /// </summary>
            Kan,
            /// <summary>
            /// The Georgian language.
            /// </summary>
            Kat,
            /// <summary>
            /// The Georgian language (old).
            /// </summary>
            Kat_Old,
            /// <summary>
            /// The Kazakh language.
            /// </summary>
            Kaz,
            /// <summary>
            /// The Central Khmer language.
            /// </summary>
            Khm,
            /// <summary>
            /// The Kirghiz/Kyrgyz language.
            /// </summary>
            Kir,
            /// <summary>
            /// The Northern Kurdish language.
            /// </summary>
            Kmr,
            /// <summary>
            /// The Korean language.
            /// </summary>
            Kor,
            /// <summary>
            /// The Korean language (vertical).
            /// </summary>
            Kor_Vert,
            /// <summary>
            /// The Lao language.
            /// </summary>
            Lao,
            /// <summary>
            /// The Latin language.
            /// </summary>
            Lat,
            /// <summary>
            /// The Latvian language.
            /// </summary>
            Lav,
            /// <summary>
            /// The Lithuanian language.
            /// </summary>
            Lit,
            /// <summary>
            /// The Luxembourgish/Letzeburgesch language.
            /// </summary>
            Ltz,
            /// <summary>
            /// The Malayalam language.
            /// </summary>
            Mal,
            /// <summary>
            /// The Marathi language.
            /// </summary>
            Mar,
            /// <summary>
            /// The Macedonian language.
            /// </summary>
            Mkd,
            /// <summary>
            /// The Maltese language.
            /// </summary>
            Mlt,
            /// <summary>
            /// The Mongolian language.
            /// </summary>
            Mon,
            /// <summary>
            /// The Maori language.
            /// </summary>
            Mri,
            /// <summary>
            /// The Malay language.
            /// </summary>
            Msa,
            /// <summary>
            /// The Burmese language.
            /// </summary>
            Mya,
            /// <summary>
            /// The Nepali language.
            /// </summary>
            Nep,
            /// <summary>
            /// The Dutch/Flemish language.
            /// </summary>
            Nld,
            /// <summary>
            /// The Norwegian language.
            /// </summary>
            Nor,
            /// <summary>
            /// The Occitan (post 1500) language.
            /// </summary>
            Oci,
            /// <summary>
            /// The Oriya language.
            /// </summary>
            Ori,
            /// <summary>
            /// The Orientation and script detection module.
            /// </summary>
            Osd,
            /// <summary>
            /// The Panjabi/Punjabi language.
            /// </summary>
            Pan,
            /// <summary>
            /// The Polish language.
            /// </summary>
            Pol,
            /// <summary>
            /// The Portuguese language.
            /// </summary>
            Por,
            /// <summary>
            /// The Pushto/Pashto language.
            /// </summary>
            Pus,
            /// <summary>
            /// The Quechua language.
            /// </summary>
            Que,
            /// <summary>
            /// The Romanian/Moldavian/Moldovan language.
            /// </summary>
            Ron,
            /// <summary>
            /// The Russian language.
            /// </summary>
            Rus,
            /// <summary>
            /// The Sanskrit language.
            /// </summary>
            San,
            /// <summary>
            /// The Sinhala/Sinhalese language.
            /// </summary>
            Sin,
            /// <summary>
            /// The Slovak language.
            /// </summary>
            Slk,
            /// <summary>
            /// The Slovenian language.
            /// </summary>
            Slv,
            /// <summary>
            /// The Sindhi language.
            /// </summary>
            Snd,
            /// <summary>
            /// The Spanish/Castilian language.
            /// </summary>
            Spa,
            /// <summary>
            /// The Spanish/Castilian language (old).
            /// </summary>
            Spa_Old,
            /// <summary>
            /// The Albanian language.
            /// </summary>
            Sqi,
            /// <summary>
            /// The Serbian language.
            /// </summary>
            Srp,
            /// <summary>
            /// The Serbian language (Latin).
            /// </summary>
            Srp_Latn,
            /// <summary>
            /// The Sundanese language.
            /// </summary>
            Sun,
            /// <summary>
            /// The Swahili language.
            /// </summary>
            Swa,
            /// <summary>
            /// The Swedish language.
            /// </summary>
            Swe,
            /// <summary>
            /// The Syriac language.
            /// </summary>
            Syr,
            /// <summary>
            /// The Tamil language.
            /// </summary>
            Tam,
            /// <summary>
            /// The Tatar language.
            /// </summary>
            Tat,
            /// <summary>
            /// The Telugu language.
            /// </summary>
            Tel,
            /// <summary>
            /// The Tajik language.
            /// </summary>
            Tgk,
            /// <summary>
            /// The Thai language.
            /// </summary>
            Tha,
            /// <summary>
            /// The Tigrinya language.
            /// </summary>
            Tir,
            /// <summary>
            /// The Tonga (Tonga Islands) language.
            /// </summary>
            Ton,
            /// <summary>
            /// The Turkish language.
            /// </summary>
            Tur,
            /// <summary>
            /// The Uighur/Uyghur language.
            /// </summary>
            Uig,
            /// <summary>
            /// The Ukrainian language.
            /// </summary>
            Ukr,
            /// <summary>
            /// The Urdu language.
            /// </summary>
            Urd,
            /// <summary>
            /// The Uzbek language.
            /// </summary>
            Uzb,
            /// <summary>
            /// The Uzbek language (Cyrillic).
            /// </summary>
            Uzb_Cyrl,
            /// <summary>
            /// The Vietnamese language.
            /// </summary>
            Vie,
            /// <summary>
            /// The Yiddish language.
            /// </summary>
            Yid,
            /// <summary>
            /// The Yoruba language.
            /// </summary>
            Yor
        }

        /// <summary>
        /// Best (most accurate) trained models. These are models for a single script supporting one or more languages.
        /// </summary>
        public enum BestScripts
        {
            /// <summary>
            /// The Arabic script.
            /// </summary>
            Arabic,
            /// <summary>
            /// The Armenian script.
            /// </summary>
            Armenian,
            /// <summary>
            /// The Bengali script.
            /// </summary>
            Bengali,
            /// <summary>
            /// The Canadian Aboriginal script.
            /// </summary>
            Canadian_Aboriginal,
            /// <summary>
            /// The Cherokee script.
            /// </summary>
            Cherokee,
            /// <summary>
            /// The Cyrillic script.
            /// </summary>
            Cyrillic,
            /// <summary>
            /// The Devanagari script.
            /// </summary>
            Devanagari,
            /// <summary>
            /// The Ethiopic script.
            /// </summary>
            Ethiopic,
            /// <summary>
            /// The Fraktur script.
            /// </summary>
            Fraktur,
            /// <summary>
            /// The Georgian script.
            /// </summary>
            Georgian,
            /// <summary>
            /// The Greek script.
            /// </summary>
            Greek,
            /// <summary>
            /// The Gujarati script.
            /// </summary>
            Gujarati,
            /// <summary>
            /// The Gurmukhi script.
            /// </summary>
            Gurmukhi,
            /// <summary>
            /// The Han (Simplified) script.
            /// </summary>
            HanS,
            /// <summary>
            /// The Han (Simplified) script. (vertical)
            /// </summary>
            HanS_Vert,
            /// <summary>
            /// The Han (Traditional) script.
            /// </summary>
            HanT,
            /// <summary>
            /// The Han (Traditional) script. (vertical)
            /// </summary>
            HanT_Vert,
            /// <summary>
            /// The Hangul script.
            /// </summary>
            Hangul,
            /// <summary>
            /// The Hangul script. (vertical)
            /// </summary>
            Hangul_Vert,
            /// <summary>
            /// The Hebrew script.
            /// </summary>
            Hebrew,
            /// <summary>
            /// The Japanese script.
            /// </summary>
            Japanese,
            /// <summary>
            /// The Japanese script. (vertical)
            /// </summary>
            Japanese_Vert,
            /// <summary>
            /// The Kannada script.
            /// </summary>
            Kannada,
            /// <summary>
            /// The Khmer script.
            /// </summary>
            Khmer,
            /// <summary>
            /// The Lao script.
            /// </summary>
            Lao,
            /// <summary>
            /// The Latin script.
            /// </summary>
            Latin,
            /// <summary>
            /// The Malayalam script.
            /// </summary>
            Malayalam,
            /// <summary>
            /// The Myanmar script.
            /// </summary>
            Myanmar,
            /// <summary>
            /// The Oriya script.
            /// </summary>
            Oriya,
            /// <summary>
            /// The Sinhala script.
            /// </summary>
            Sinhala,
            /// <summary>
            /// The Syriac script.
            /// </summary>
            Syriac,
            /// <summary>
            /// The Tamil script.
            /// </summary>
            Tamil,
            /// <summary>
            /// The Telugu script.
            /// </summary>
            Telugu,
            /// <summary>
            /// The Thaana script.
            /// </summary>
            Thaana,
            /// <summary>
            /// The Thai script.
            /// </summary>
            Thai,
            /// <summary>
            /// The Tibetan script.
            /// </summary>
            Tibetan,
            /// <summary>
            /// The Vietnamese script.
            /// </summary>
            Vietnamese
        }

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using the provided <paramref name="prefix"/> and <paramref name="language"/> name, without processing them in any way. 
        /// </summary>
        /// <param name="prefix">The name of the folder where the language file is located. If this is <see langword="null" />, the value of the environment variable <c>TESSDATA_PREFIX</c> will be used.</param>
        /// <param name="language">The name of the language. The Tesseract library will assume that the trained language data file can be found at <paramref name="prefix"/><c>/</c><paramref name="language"/><c>.traineddata</c>.</param>
        public TesseractLanguage(string prefix, string language)
        {
            this.Prefix = prefix;
            this.Language = language;
        }

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using the specified trained model data file.
        /// </summary>
        /// <param name="fileName">The path to the trained model data file. If the file name does not end in <c>.traineddata</c>, the file is copied to a temporary folder, and the temporary file is used by the Tesseract library.</param>
        public TesseractLanguage(string fileName)
        {
            if (fileName.EndsWith(".traineddata"))
            {
                fileName = Path.GetFullPath(fileName);

                this.Prefix = Path.GetDirectoryName(fileName);
                this.Language = Path.GetFileName(fileName).Substring(0, Path.GetFileName(fileName).Length - 12);
            }
            else
            {
                this.Prefix = Path.GetTempPath();
                this.Language = Guid.NewGuid().ToString("N");

                File.Copy(fileName, Path.Combine(this.Prefix, this.Language + ".traineddata"));
            }

        }

        private static readonly string ExecutablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string LocalCachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using a fast integer version of a trained model for the specified language. The language file is downloaded from the <c>tesseract-ocr/tessdata_fast</c> GitHub repository. If it has already been downloaded and cached before, the downloaded file is re-used.
        /// </summary>
        /// <param name="language">The language to use for the OCR process.</param>
        /// <param name="useAnyCached">If this is <see langword="true"/>, if a cached trained model file is available for the specified language, it will be used even if it is a "best (most accurate)" model. Otherwise, only cached fast integer trained models will be used.</param>
        public TesseractLanguage(Fast language, bool useAnyCached = false)
        {
            string languageName = language.ToString().ToLower();

            string prefix = null;

            if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "fast", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "tessdata", "fast");
            }
            else if (File.Exists(Path.Combine(ExecutablePath, "fast", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "fast");
            }
            else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "fast", languageName + ".traineddata")))
            {
                prefix = Path.Combine(LocalCachePath, "tessdata", "fast");
            }
            else if (useAnyCached)
            {
                if (File.Exists(Path.Combine(ExecutablePath, languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath);
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "best", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "tessdata", "best");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "best", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "best");
                }
                else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "best", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(LocalCachePath, "tessdata", "best");
                }
            }

            if (prefix == null)
            {
                string remotePath = "https://github.com/tesseract-ocr/tessdata_fast/raw/main/" + languageName + ".traineddata";

                string localDirectory = Path.Combine(LocalCachePath, "tessdata", "fast");

                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(remotePath, Path.Combine(localDirectory, languageName + ".traineddata"));
                }

                prefix = localDirectory;
            }

            this.Prefix = prefix;
            this.Language = languageName;
        }

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using the best (most accurate) version of the trained model for the specified language. The language file is downloaded from the <c>tesseract-ocr/tessdata_best</c> GitHub repository. If it has already been downloaded and cached before, the downloaded file is re-used.
        /// </summary>
        /// <param name="language">The language to use for the OCR process.</param>
        /// <param name="useAnyCached">If this is <see langword="true"/>, if a cached trained model file is available for the specified language, it will be used even if it is a "fast" model. Otherwise, only cached best (most accurate) trained models will be used.</param>
        public TesseractLanguage(Best language, bool useAnyCached = false)
        {
            string languageName = language.ToString().ToLower();

            string prefix = null;

            if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "best", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "tessdata", "best");
            }
            else if (File.Exists(Path.Combine(ExecutablePath, "best", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "best");
            }
            else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "best", languageName + ".traineddata")))
            {
                prefix = Path.Combine(LocalCachePath, "tessdata", "best");
            }
            else if (useAnyCached)
            {
                if (File.Exists(Path.Combine(ExecutablePath, languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath);
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "fast", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "tessdata", "fast");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "fast", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "fast");
                }
                else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "fast", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(LocalCachePath, "tessdata", "fast");
                }
            }

            if (prefix == null)
            {
                string remotePath = "https://github.com/tesseract-ocr/tessdata_best/raw/main/" + languageName + ".traineddata";

                string localDirectory = Path.Combine(LocalCachePath, "tessdata", "best");

                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(remotePath, Path.Combine(localDirectory, languageName + ".traineddata"));
                }

                prefix = localDirectory;
            }

            this.Prefix = prefix;
            this.Language = languageName;
        }

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using a fast integer version of a trained model for the specified script. The language file is downloaded from the <c>tesseract-ocr/tessdata_fast</c> GitHub repository. If it has already been downloaded and cached before, the downloaded file is re-used.
        /// </summary>
        /// <param name="script">The script to use for the OCR process.</param>
        /// <param name="useAnyCached">If this is <see langword="true"/>, if a cached trained model file is available for the specified script, it will be used even if it is a "best (most accurate)" model. Otherwise, only cached fast integer trained models will be used.</param>
        public TesseractLanguage(FastScripts script, bool useAnyCached = false)
        {
            string languageName = script.ToString().Replace("_Vert", "_vert");

            string prefix = null;

            if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "fast", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "tessdata", "fast", "script");
            }
            else if (File.Exists(Path.Combine(ExecutablePath, "fast", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "fast", "script");
            }
            else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "fast", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(LocalCachePath, "tessdata", "fast", "script");
            }
            else if (useAnyCached)
            {
                if (File.Exists(Path.Combine(ExecutablePath, "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "script");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath);
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "best", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "tessdata", "best", "script");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "best", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "best", "script");
                }
                else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "best", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(LocalCachePath, "tessdata", "best", "script");
                }
            }

            if (prefix == null)
            {
                string remotePath = "https://github.com/tesseract-ocr/tessdata_fast/raw/main/script/" + languageName + ".traineddata";

                string localDirectory = Path.Combine(LocalCachePath, "tessdata", "fast", "script");

                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(remotePath, Path.Combine(localDirectory, languageName + ".traineddata"));
                }

                prefix = localDirectory;
            }

            this.Prefix = prefix;
            this.Language = languageName;
        }

        /// <summary>
        /// Create a new <see cref="TesseractLanguage"/> object using the best (most accurate) version of the trained model for the specified script. The language file is downloaded from the <c>tesseract-ocr/tessdata_best</c> GitHub repository. If it has already been downloaded and cached before, the downloaded file is re-used.
        /// </summary>
        /// <param name="script">The script to use for the OCR process.</param>
        /// <param name="useAnyCached">If this is <see langword="true"/>, if a cached trained model file is available for the specified script, it will be used even if it is a "fast" model. Otherwise, only cached best (most accurate) trained models will be used.</param>
        public TesseractLanguage(BestScripts script, bool useAnyCached = false)
        {
            string languageName = script.ToString().Replace("_Vert", "_vert");

            string prefix = null;

            if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "best", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "tessdata", "best", "script");
            }
            else if (File.Exists(Path.Combine(ExecutablePath, "best", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(ExecutablePath, "best", "script");
            }
            else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "best", "script", languageName + ".traineddata")))
            {
                prefix = Path.Combine(LocalCachePath, "tessdata", "best", "script");
            }
            else if (useAnyCached)
            {
                if (File.Exists(Path.Combine(ExecutablePath, "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "script");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath);
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "tessdata", "fast", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "tessdata", "fast", "script");
                }
                else if (File.Exists(Path.Combine(ExecutablePath, "fast", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(ExecutablePath, "fast", "script");
                }
                else if (File.Exists(Path.Combine(LocalCachePath, "tessdata", "fast", "script", languageName + ".traineddata")))
                {
                    prefix = Path.Combine(LocalCachePath, "tessdata", "fast", "script");
                }
            }

            if (prefix == null)
            {
                string remotePath = "https://github.com/tesseract-ocr/tessdata_best/raw/main/script/" + languageName + ".traineddata";

                string localDirectory = Path.Combine(LocalCachePath, "tessdata", "best", "script");

                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(remotePath, Path.Combine(localDirectory, languageName + ".traineddata"));
                }

                prefix = localDirectory;
            }

            this.Prefix = prefix;
            this.Language = languageName;
        }
    }
}
