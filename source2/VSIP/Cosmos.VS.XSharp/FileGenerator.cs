﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.IO;

namespace Cosmos.VS.XSharp {
  // This is the custom tool used to compile .XS files to .CS files in VS
  public class FileGenerator : IVsSingleFileGenerator {
    public int DefaultExtension(out string aDefaultExt) {
      aDefaultExt = ".cs";
      return VSConstants.S_OK;
    }

    public int Generate(string aInputFilePath, string aInputFileContents, string aDefaultNamespace, IntPtr[] aOutputFileContents, out uint oPcbOutput, IVsGeneratorProgress aGenerateProgress) {
      var xGeneratedCode = DoGenerate(aInputFilePath, aInputFileContents, aDefaultNamespace);
      var xBytes = Encoding.UTF8.GetBytes(xGeneratedCode);

      if (xBytes.Length > 0) {
        aOutputFileContents[0] = Marshal.AllocCoTaskMem(xBytes.Length);
        Marshal.Copy(xBytes, 0, aOutputFileContents[0], xBytes.Length);
        oPcbOutput = (uint)xBytes.Length;
      } else {
        aOutputFileContents[0] = IntPtr.Zero;
        oPcbOutput = 0;
      }
      return VSConstants.S_OK;
    }

    private static string DoGenerate(string aInputFileName, string aInputFileContents, string aDefaultNamespace) {
      string xClassName = Path.GetFileNameWithoutExtension(aInputFileName);
      using (var xInput = new StringReader(aInputFileContents)) {
        using (var xOutput = new StringWriter()) {
          try {
            var xGenerator = new Cosmos.Compiler.XSharp.CSharpGenerator();
            xGenerator.Execute(aDefaultNamespace, xClassName, xInput, xOutput);
            return xOutput.ToString();
          } catch (Exception ex) {
            return xOutput.ToString() + "\r\n"
              + ex.Message + "\r\n";
          }
        }
      }
    }
  }
}