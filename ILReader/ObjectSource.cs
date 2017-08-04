using System;
using System.Reflection.Emit;
using System.IO;
using System.Collections.Generic;
using ClrTest.Reflection;
using System.Text;

namespace ClrTest.Reflection
{
    public class ObjectSource
    {
        public static string GetData(DynamicMethod method)
        {
            /*
                        lblMethodGetType.Text = m_mbi.TypeName;
                        txtMethodToString.Text = m_mbi.MethodToString;
                        lblFixupSucceed.Text = "Try fix labels: " + (m_mbi.FixupSuccess ? "succeed" : "failed");
                        if (verbose)
                        {
                            if (m_longVersion == null)
                                m_longVersion = m_mbi.Instructions.ToArray();
                            richTextBox1.Lines = m_longVersion;
                        }
                        else
                        {
                            if (m_shortVersion == null)
                            {
                                int count = m_mbi.Instructions.Count;
                                m_shortVersion = new string[count];
                                for (int i = 0; i < count; i++)
                                {
                                    m_shortVersion[i] = m_mbi.Instructions[i].Remove(9, 20);
                                }
                            }
                            richTextBox1.Lines = m_shortVersion;
                        }
             */
            /*
            MethodBodyInfo info = new MethodBodyInfo();
            info.TypeName = method.GetType().Name;
            info.MethodToString = method.ToString();

            ILReader reader = new ILReader(method);
            foreach (ILInstruction instr in reader)
                info.Instructions.Add(instr.ToString());

            info.FixupSuccess = reader.FixupSuccess;
            */


            var reader = new ILReader(method);
            var text = new StringBuilder();

            text.Append(method.GetType().Name); text.Append(Environment.NewLine);
            text.Append(method.ToString()); text.Append(Environment.NewLine);
            text.Append("Try fix labels: " + (reader.FixupSuccess ? "succeed" : "failed")); text.Append(Environment.NewLine); text.Append(Environment.NewLine);

            foreach (ILInstruction instr in reader)
            {
                text.Append(instr.ToString()); text.Append(Environment.NewLine);
            }

            return text.ToString();
        }
    }
}