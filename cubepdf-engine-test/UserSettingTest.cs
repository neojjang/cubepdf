﻿/* ------------------------------------------------------------------------- */
/*
 *  UserSettingTest.cs
 *
 *  Copyright (c) 2009 CubeSoft, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see < http://www.gnu.org/licenses/ >.
 */
/* ------------------------------------------------------------------------- */
using System;
using System.IO;
using NUnit.Framework;
using Microsoft.Win32;

namespace CubePDF
{
    /* --------------------------------------------------------------------- */
    ///
    /// UserSettingSaver
    /// 
    /// <summary>
    /// テストクラスでレジストリの値を変更するので、元の値を保存、復元する
    /// ためのクラス。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    class UserSettingSaver : IDisposable
    {
        /* ----------------------------------------------------------------- */
        /// Constructor
        /* ----------------------------------------------------------------- */
        public UserSettingSaver()
        {
            _setting = new UserSetting(true);
        }

        /* ----------------------------------------------------------------- */
        /// Dispose
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        /// 
        /// <summary>
        /// レジストリの値をコンストラクタが呼び出された時点に戻す。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_setting != null)
                    {
                        _setting.Save();
                        _setting = null;
                    }
                }
            }
            _disposed = true;
        }

        /* ----------------------------------------------------------------- */
        /// 変数定義
        /* ----------------------------------------------------------------- */
        #region Variables
        private UserSetting _setting = null;
        private bool _disposed = false;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// UserSettingTest
    ///
    /// <summary>
    /// ユーザ設定のロード/セーブをテストするためのクラス。
    /// 現バージョンでは、ユーザ設定は全てレジストリに保存されている。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class UserSettingTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestLoadValue
        /// 
        /// <summary>
        /// レジストリから値をロードするテスト。
        /// 
        /// NOTE: UserSetting クラスは、コンストラクタで（true を指定して）
        /// ロードする方法と、Load() メソッドを明示的にコールしてロードする
        /// 方法の 2 種類が存在する
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestLoadValue()
        {
            try
            {
                var load_from_constructor = new UserSetting(true);
                load_from_constructor = null;

                var load_from_method = new UserSetting(false);
                Assert.IsTrue(load_from_method.Load(), "Load from registry");
                load_from_method = null;
            }
            catch (Exception err)
            {
                Assert.Fail(err.ToString());
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestSaveDefaultValue
        /// 
        /// <summary>
        /// デフォルト値で保存するテスト。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestSaveDefaultValue()
        {
            using (var saver = new UserSettingSaver())
            {
                var default_value = new UserSetting(false);
                Assert.IsTrue(default_value.Save(), "Save to registry");

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var test = new UserSetting(false);
                Assert.IsTrue(test.Load(), "Load from registry");

                // 各種デフォルト値のテスト
                Assert.AreEqual(desktop, test.OutputPath, "LastAccess");
                Assert.AreEqual(desktop, test.InputPath, "LastInputAccess");
                Assert.AreEqual("", test.UserProgram, "UserProgram");
                Assert.AreEqual("%%FILE%%", test.UserArguments, "UserArguments");
                Assert.AreEqual(Parameter.FileTypes.PDF, test.FileType, "FileType");
                Assert.AreEqual(Parameter.PDFVersions.Ver1_7, test.PDFVersion, "PDFVersion");
                Assert.AreEqual(Parameter.Resolutions.Resolution300, test.Resolution, "Resolution");
                Assert.AreEqual(Parameter.ExistedFiles.Overwrite, test.ExistedFile, "ExistedFile");
                Assert.AreEqual(Parameter.PostProcesses.Open, test.PostProcess, "PostProcess");
                Assert.AreEqual(Parameter.ImageFilters.FlateEncode, test.ImageFilter, "ImageFilter");
                Assert.IsTrue(test.PageRotation, "PageRotation"); // ページの自動回転
                Assert.IsTrue(test.EmbedFont, "EmbedFont"); // フォントの埋め込み
                Assert.IsFalse(test.Grayscale, "Grayscale"); // グレースケール
                Assert.IsFalse(test.WebOptimize, "WebOptimize"); // Web 表示用に最適化
                Assert.IsFalse(test.SaveSetting, "SaveSetting"); // 設定を保存する
                Assert.IsTrue(test.CheckUpdate, "CheckUpdate"); // 起動時にアップデートを確認する
                Assert.IsFalse(test.AdvancedMode, "AdvancedMode"); // ポストプロセスでユーザ―プログラムを選択可能にする
                Assert.IsFalse(test.SelectInputFile, "SelectInputFile"); // 入力ファイル欄を表示
                Assert.IsFalse(test.DeleteOnClose, "DeleteOnClose"); // 終了時に入力ファイルを消去（レジストリには項目なし）
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestRegisterUpdateChecker
        /// 
        /// <summary>
        /// アップデートチェッカが
        /// HKCU\Software\Microsoft\Windows\CurrentVersion\Run 下に登録
        /// されたかどうかをチェックするテスト。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestRegisterUpdateChecker()
        {
            using (var saver = new UserSettingSaver())
            {
                var test = new UserSetting(false);
                Assert.IsTrue(test.Load(), "Load from registry");
                test.CheckUpdate = true;
                Assert.IsTrue(test.Save(), "Save to registry (CheckUpdate = true)");

                {
                    var subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                    Assert.IsTrue(subkey != null, @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    Assert.IsTrue(subkey.GetValue("cubepdf-checker") != null);
                }

                test.CheckUpdate = false;
                Assert.IsTrue(test.Save(), "Save to registry (CheckUpdate = false)");

                {
                    var subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                    Assert.IsTrue(subkey != null, @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    Assert.IsTrue(subkey.GetValue("cubepdf-checker") == null);
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestSaveNotFoundPath
        /// 
        /// <summary>
        /// 入力パス、および出力パスを存在しないパスで保存した場合のテスト。
        /// 
        /// NOTE: 現バージョンでは、UserSetting では特別な処理を行わずに
        /// 指定された値をそのままレジストリに保存する。存在チェック等は、
        /// UserSetting クラスの外側で行う。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestSaveNotFoundPath()
        {
            using (var saver = new UserSettingSaver())
            {
                var test = new UserSetting(false);
                Assert.IsTrue(test.Load(), "Load from registry");

                string not_found = @"C:\404_notfound\foo\bar\bas\foo.txt";
                test.OutputPath = not_found;
                test.InputPath = not_found;
                Assert.IsTrue(test.Save(), "Save to registry");

                Assert.IsTrue(test.Load(), "Load from registry (second)");
                Assert.AreEqual(not_found, test.OutputPath, "LastAccess");
                Assert.AreEqual(not_found, test.InputPath, "LastInputAccess");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestSaveEmptyString
        /// 
        /// <summary>
        /// レジストリの文字列型の値に空の文字列を保存した場合のテスト。
        /// 空の文字列を保存し、再度ロードした時の値は以下のようになる。
        /// 
        /// InputPath       : デスクトップへのパス
        /// OutputPath      : デスクトップへのパス
        /// UserProgram     : 空文字のまま
        /// UserArguments   : 空文字のまま
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestSaveEmptyString()
        {
            using (var saver = new UserSettingSaver())
            {
                string dummy = "dummy";
                var test = new UserSetting(false);
                test.InputPath = dummy;
                test.OutputPath = dummy;
                test.UserProgram = dummy;
                test.UserArguments = dummy;
                Assert.IsTrue(test.Save(), "Save from registry");
                Assert.IsTrue(test.Load(), "Load from registry");
                Assert.AreEqual(dummy, test.OutputPath, "LastAccess");
                Assert.AreEqual(dummy, test.InputPath, "LastInputAccess");
                Assert.AreEqual(dummy, test.UserProgram, "UserProgram");
                Assert.AreEqual(dummy, test.UserArguments, "UserArguments");

                string empty = "";
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                test.InputPath = empty;
                test.OutputPath = empty;
                test.UserProgram = empty;
                test.UserArguments = empty;
                Assert.IsTrue(test.Save(), "Save from registry (second)");
                Assert.IsTrue(test.Load(), "Load from registry (second)");
                Assert.AreEqual(desktop, test.OutputPath, "LastAccess"); // empty ではない
                Assert.AreEqual(desktop, test.InputPath, "LastInputAccess"); // empty ではない
                Assert.AreEqual(empty, test.UserProgram, "UserProgram");
                Assert.AreEqual(empty, test.UserArguments, "UserArguments");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestDeleteStringValue
        /// 
        /// <summary>
        /// レジストリの文字列型の値を消去したときのテスト。
        /// 空を消去して、再度ロードした時の値は以下のようになる（空文字で
        /// 保存した場合と、結果が若干異なる）。
        /// 
        /// InputPath       : デスクトップへのパス
        /// OutputPath      : デスクトップへのパス
        /// UserProgram     : 空文字のまま
        /// UserArguments   : %%FILE%%
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestDeleteStringValue()
        {
            using (var saver = new UserSettingSaver())
            {
                // 値をレジストリ操作で無理やり消す。
                try
                {
                    var subkey = Registry.CurrentUser.OpenSubKey(@"Software\CubeSoft\CubePDF\v2", true);
                    Assert.IsTrue(subkey != null, @"HKCU\Software\CubeSoft\CubePDF\v2");
                    subkey.DeleteValue("LastAccess", false);
                    subkey.DeleteValue("LastInputAccess", false);
                    subkey.DeleteValue("UserProgram", false);
                    subkey.DeleteValue("UserArguments", false);
                }
                catch (Exception err)
                {
                    Assert.Fail(err.ToString());
                }

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); 
                var test = new UserSetting(false);
                Assert.IsTrue(test.Load(), "Load from registry (second)");
                Assert.AreEqual(desktop, test.OutputPath, "LastAccess");
                Assert.AreEqual(desktop, test.InputPath, "LastInputAccess");
                Assert.AreEqual("", test.UserProgram, "UserProgram");
                Assert.AreEqual("%%FILE%%", test.UserArguments, "UserArguments");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestSaveInvalidValue
        /// 
        /// <summary>
        /// レジストリに不正な値が設定されている場合のテスト。
        /// 不正な値が設定されている場合の挙動は以下の通り。
        /// 
        /// コンボボックス   : 不正な値は読み飛ばして、デフォルト値を使用
        /// チェックボックス : 0 以外の場合は true として扱う
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestSaveInvalidValue()
        {
            using (var saver = new UserSettingSaver())
            {
                try
                {
                    var subkey = Registry.CurrentUser.OpenSubKey(@"Software\CubeSoft\CubePDF\v2", true);
                    Assert.IsTrue(subkey != null, @"HKCU\Software\CubeSoft\CubePDF\v2");

                    // コンボボックスの値（ImageFilter のみ GUI 上はチェックボックス）
                    subkey.SetValue("FileType", 256);
                    subkey.SetValue("PDFVersion", 1024);
                    subkey.SetValue("Resolution", 5012);
                    subkey.SetValue("ExistedFile", 8252);
                    subkey.SetValue("PostProcess", 2958739);
                    subkey.SetValue("DownSampling", 493798);
                    subkey.SetValue("ImageFilter", 943724);

                    // チェックボックスの値（正常な値は、0 or 1）
                    subkey.SetValue("PageRotation", 2);
                    subkey.SetValue("EmbedFont", 5);
                    subkey.SetValue("Grayscale", 8);
                    subkey.SetValue("WebOptimize", 12);
                    subkey.SetValue("SaveSetting", 24);
                    subkey.SetValue("CheckUpdate", 32);
                    subkey.SetValue("AdvancedMode", 42);
                    subkey.SetValue("SelectInputFile", 128);
                }
                catch (Exception err)
                {
                    Assert.Fail(err.ToString());
                }

                var test = new UserSetting(false);
                Assert.IsTrue(test.Load(), "Load from registry");
                Assert.AreEqual(Parameter.FileTypes.PDF, test.FileType, "FileType");
                Assert.AreEqual(Parameter.PDFVersions.Ver1_7, test.PDFVersion, "PDFVersion");
                Assert.AreEqual(Parameter.Resolutions.Resolution300, test.Resolution, "Resolution");
                Assert.AreEqual(Parameter.ExistedFiles.Overwrite, test.ExistedFile, "ExistedFile");
                Assert.AreEqual(Parameter.PostProcesses.Open, test.PostProcess, "PostProcess");
                Assert.AreEqual(Parameter.ImageFilters.FlateEncode, test.ImageFilter, "ImageFilter");
                Assert.IsTrue(test.PageRotation, "PageRotation");
                Assert.IsTrue(test.EmbedFont, "EmbedFont");
                Assert.IsTrue(test.Grayscale, "Grayscale");
                Assert.IsTrue(test.WebOptimize, "WebOptimize");
                Assert.IsTrue(test.SaveSetting, "SaveSetting");
                Assert.IsTrue(test.CheckUpdate, "CheckUpdate");
                Assert.IsTrue(test.AdvancedMode, "AdvancedMode");
                Assert.IsTrue(test.SelectInputFile, "SelectInputFile");
            }
        }
    }
}
