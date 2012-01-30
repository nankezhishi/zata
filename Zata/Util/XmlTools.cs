using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Zata.Util
{


	/// <summary>
	/// XmlTools ��ժҪ˵����
	/// </summary>
	public class XmlTools
	{
		#region ��ȡXML������

		private static Dictionary<string, XmlSerializer> XmlSerializerDict = new Dictionary<string, XmlSerializer>();

        #region �õ�XML������, ָ����������Ĭ��Ԫ��ͷ�ļ�

        /// <summary>
        /// �õ�XML������, ָ����������Ĭ��Ԫ��ͷ�ļ�
        /// </summary>
        /// <param name="typeXS"></param>
        /// <param name="rootnodeName"></param>
        /// <returns></returns>
        public static XmlSerializer GetXmlSerializer(Type typeXS, string rootnodeName)
        {
            return GetXmlSerializerInternal(typeXS, rootnodeName, false);
        }

        #endregion

        #region �õ�XML������

        /// <summary>
        /// �õ�XML������
        /// </summary>
        /// <param name="typeXS"></param>
        /// <returns></returns>
        public static XmlSerializer GetXmlSerializer(Type typeXS)
        {
            return GetXmlSerializerInternal(typeXS, string.Empty, false);
        }

        #endregion

        #region �õ�SOAP XML������, ָ����������Ĭ��Ԫ��ͷ�ļ�

        /// <summary>
        /// �õ�SOAP XML������, ָ����������Ĭ��Ԫ��ͷ�ļ�
        /// </summary>
        /// <param name="typeXS"></param>
        /// <param name="rootnodeName"></param>
        /// <returns></returns>
        public static XmlSerializer GetSoapSerializer(Type typeXS, string rootnodeName)
        {
            return GetXmlSerializerInternal(typeXS, rootnodeName, true);
        }

        #endregion

        #region �õ�SOAP XML������

        /// <summary>
        /// �õ�SOAP XML������
        /// </summary>
        /// <param name="typeXS"></param>
        /// <returns></returns>
        public static XmlSerializer GetSoapSerializer(Type typeXS)
        {
            return GetXmlSerializerInternal(typeXS, string.Empty, true);
        }

        #endregion

        #region Xml�������ڲ�����ʵ��

        /// <summary>
		/// ����XmlSerializer����,�Ա���XmlSerializer����ʱ�Զ�����
		/// </summary>
		/// <param name="typeXS">XmlSerializer����</param>
		/// <param name="rootnodeName">��Ԫ������</param>
		/// <param name="IsSoapXml">�Ƿ�����Soap��ʽ��Xml�ļ�</param>
		/// <returns>XmlSerializer����</returns>
		private static XmlSerializer GetXmlSerializerInternal(Type typeXS, string rootnodeName, bool IsSoapXml)
		{
			string keyHsXS = string.Format("{0}_{1}_{2}_{3}", typeXS.AssemblyQualifiedName, typeXS.FullName, rootnodeName, IsSoapXml);

            if (XmlSerializerDict.ContainsKey(keyHsXS))
            {
                return XmlSerializerDict[keyHsXS];
            }

			lock (XmlSerializerDict)
			{
				if (XmlSerializerDict.ContainsKey(keyHsXS))
				{
                    return XmlSerializerDict[keyHsXS];
				}
				else
				{
                    XmlSerializer rtVal = GetSerilizer(typeXS, rootnodeName, IsSoapXml);

					XmlSerializerDict.Add(keyHsXS, rtVal);

                    return rtVal;
				}
			}
        }

        private static XmlSerializer GetSerilizer(Type typeXS, string rootnodeName, bool IsSoapXml)
        {
            XmlSerializer rtVal;
            if (IsSoapXml)
            {
                XmlTypeMapping myTypeMapping = (new SoapReflectionImporter()).ImportTypeMapping(typeXS);
                rtVal = new XmlSerializer(myTypeMapping);
            }
            else
            {
                if (!string.IsNullOrEmpty(rootnodeName))
                {
                    rtVal = new XmlSerializer(typeXS, new XmlRootAttribute(rootnodeName));
                }
                else
                {
                    rtVal = new XmlSerializer(typeXS);
                }
            }
            return rtVal;
        }

        #endregion

		#endregion

		#region ��XmlReader�л�ȡָ���ڵ��ָ�����Ե�ֵ

		/// <summary>
		/// ��XmlReader�л�ȡָ���ڵ��ָ�����Ե�ֵ, 
		/// �˷�����ȡ��ƪ�ĵ���δ�����쳣��δ��λ
		/// </summary>
		/// <param name="xtr"></param>
		/// <param name="ElementName"></param>
		/// <param name="AttributeName"></param>
		/// <returns>ָ���ڵ��ָ�����Ե�ֵ�����û���ҵ������ؿ�</returns>
		public static string GetAttributeName(XmlReader xtr, string ElementName, string AttributeName)
		{
			xtr.MoveToFirstAttribute();
			while (xtr.Read())
			{
				if (xtr.Name == ElementName && xtr.IsStartElement())
				{
					return xtr.GetAttribute(AttributeName);
				}
			}

			return string.Empty;
		}

		#endregion

        #region XML���л�

        #region �ڲ�ʵ��, �������ʽ��ΪXml�ַ���

        /// <summary>
		/// �������ʽ��ΪXml�ַ���
		/// </summary>
		/// <param name="o">��Ҫ���л��Ķ���ʵ��</param>
		/// <param name="rootnodeName">��Ԫ������</param>
		/// <param name="IsCompleteXml">�Ƿ�����������Xml�ļ�(����XMLͷ)</param>
		/// <param name="IsSoapXml">�Ƿ�����Soap��ʽ��Xml�ļ�</param>
        /// <param name="IsIndent">�Ƿ�����</param>
        public static string ToXmlInternal(object o, string rootnodeName, bool IsCompleteXml, bool IsSoapXml, bool IsIndent)
		{
			if (o == null)
			{
				return string.Empty;
			}

			Type t = o.GetType();
			XmlSerializer xs = GetXmlSerializerInternal(t, rootnodeName, IsSoapXml);
            
			StringBuilder sbXml = new StringBuilder();

            XmlSerializerNamespaces xns = new XmlSerializerNamespaces();
            xns.Add("", "");

            using (MemoryStream ms = new MemoryStream())
            {
                //2011-03-24 ���� ��������������, 1.XML��������, 2.ȥ����UTF8ǰ���BOM�ַ�
                using (XmlWriter xw = XmlWriter.Create(ms, GetXmlWriterSettings(IsCompleteXml, IsIndent)))
                {
                    xs.Serialize(xw, o, xns);

                    sbXml.Append(Encoding.UTF8.GetString(ms.ToArray()));
                }
            }

            return sbXml.ToString();
		}

        /// <summary>
        /// ��ȡXML��ʽ��ѡ��
        /// </summary>
        /// <param name="IsCompleteXml"></param>
        /// <returns></returns>
        /// <param name="IsIndent">�Ƿ�����</param>
        private static XmlWriterSettings GetXmlWriterSettings(bool IsCompleteXml, bool IsIndent)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = IsIndent;
            xws.OmitXmlDeclaration = !IsCompleteXml;
            xws.CheckCharacters = false;
            xws.Encoding = new UTF8Encoding(false);
            return xws;
        }

        #endregion

        #region XML���л�(����Ĭ�ϸ�Ԫ������)


        /// <summary>
        /// XML���л�(����Ĭ�ϸ�Ԫ������)
        /// </summary>
        /// <param name="o">��Ҫ���л��Ķ���ʵ��</param>
        /// <param name="IsCompleteXml">�Ƿ�����������Xml�ļ�(����XMLͷ)</param>
		public static string ToXml(object o, bool IsCompleteXml)
		{
            return ToXmlInternal(o, string.Empty, IsCompleteXml, false, false);
        }

        #endregion

        #region XML���л�

        /// <summary>
        /// XML���л�
        /// </summary>
        /// <param name="o">��Ҫ���л��Ķ���ʵ��</param>
        /// <param name="rootnodeName">��Ԫ������</param>
        /// <param name="IsCompleteXml">�Ƿ�����������Xml�ļ�(����XMLͷ)</param>
        public static string ToXml(object o, string rootnodeName, bool IsCompleteXml)
		{
            return ToXmlInternal(o, rootnodeName, IsCompleteXml, false, false);
        }

        #endregion

        #region XML���л�(����XML, �Զ����Ԫ������)

        /// <summary>
        /// XML���л�(����XML, �Զ����Ԫ������)
        /// </summary>
        /// <param name="o">��Ҫ���л��Ķ���ʵ��</param>
        /// <param name="rootnodeName">��Ԫ������</param>
        public static string ToXml(object o, string rootnodeName)
		{
            return ToXmlInternal(o, rootnodeName, true, false, false);
        }

        #endregion

        #region XML���л�(ȫ������Ĭ������)

        /// <summary>
        /// XML���л�(ȫ������Ĭ������)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
		public static string ToXml(object o)
		{
            return ToXmlInternal(o, string.Empty, true, false, false);
		}

		#endregion

        #region SOAP���л�

        /// <summary>
        /// SOAP���л�
        /// </summary>
        /// <param name="o">��Ҫ���л��Ķ���ʵ��</param>
        /// <param name="rootnodeName">��Ԫ������</param>
        public static string ToSoap(object o, string rootnodeName)
        {
            return ToXmlInternal(o, rootnodeName, true, true, false);
        }

        #endregion

        #region SOAP���л�(ȫ������Ĭ������)

        /// <summary>
        /// SOAP���л�(ȫ������Ĭ������)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToSoap(object o)
        {
            return ToXmlInternal(o, string.Empty, true, true, false);
        }

        #endregion

        #endregion

        #region ��Xml�ַ��������л�Ϊ����

        /// <summary>
        /// ��Xml�ַ��������л�Ϊ����
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static object FromXml(string xml, Type objType)
		{
            object rtVal = null;
            byte[] bytes = Encoding.UTF8.GetBytes(ReplaceXmlString(xml));

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (StreamReader sr = new StreamReader(ms, Encoding.UTF8))
                {
                    XmlSerializer serializer = GetXmlSerializer(objType);
                    
                    rtVal = serializer.Deserialize(sr);
                }
            }

            return rtVal;
		}

        /// <summary>
        /// ��Xml�ڷ����л�Ϊ����
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static object FromXml(XmlNode xmlNode, Type objType)
        {
            object rtVal = null;

            using (XmlNodeReader ms = new XmlNodeReader(xmlNode))
            {
                XmlSerializer serializer = GetXmlSerializer(objType);

                rtVal = serializer.Deserialize(ms);
            }

            return rtVal;
        }

        /// <summary>
        /// ��Xml�ַ��������л�Ϊ����
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T FromXml<T>(string xml) where  T: class
        {
            return FromXml(xml, typeof(T)) as T;
        }

        /// <summary>
        /// ��Xml�ַ��������л�Ϊ����
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static T FromXml<T>(XmlNode xmlNode) where T : class
        {
            return FromXml(xmlNode, typeof(T)) as T;
        }

		#endregion

        #region ʹ��XSLת��XML����HTML�ַ���

        /// <summary>
		/// ʹ��XSLת��XML����HTML�ַ���
		/// </summary>
		/// <param name="xml">xml����</param>
		/// <param name="xsl">xsl����</param>
		/// <param name="isXmlUri">ʹ��URI��Ϊxml����</param>
		/// <param name="isXslUri">ʹ��URI��Ϊxsl����</param>
		/// <returns></returns>
		public static string XslTransferXml(string xml, string xsl, bool isXmlUri, bool isXslUri)
		{
			StringBuilder sbRet = new StringBuilder();
			XslCompiledTransform xslDoc = new XslCompiledTransform();
			System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();

			if (isXslUri)
			{
				xslDoc.Load(xsl);
			}
			else
			{
				xmlDoc.LoadXml(xsl);
				xslDoc.Load(xmlDoc);
			}

			if (isXmlUri)
			{
				xmlDoc.Load(xml);
			}
			else
			{
				xmlDoc.LoadXml(xml);
			}

			using (System.IO.StringWriter sw = new System.IO.StringWriter(sbRet))
			{
				xslDoc.Transform(xmlDoc, null, sw);
				sw.Close();
			}

			return sbRet.ToString();
		}

        /// <summary>
        /// ʹ��XSLת��XML����HTML�ַ���
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xslUri"></param>
        /// <returns></returns>
		public static string XslTransferXml(string xml, string xslUri)
		{
			return XslTransferXml(xml, xslUri, false, true);
		}

		#endregion

        #region ��ȡָ��·���µ�InnerText

        /// <summary>
        /// ��ȡָ��·���µ�InnerText
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xPath"></param>
        /// <param name="IsThrowError">���û���ҵ��Ƿ��׳�����</param>
        /// <returns></returns>
        public static string GetInnerText(XmlDocument xml, string xPath, bool IsThrowError)
        {
            XmlNode node = xml.SelectSingleNode(xPath);

            if (node == null && IsThrowError == true)
                throw new XmlToolsException(string.Format("û����Xml�ĵ����ҵ�·��Ϊ{0}�Ľڵ�", xPath));

            if (node == null) return string.Empty;
            
            return node.InnerText;
        }

        /// <summary>
        /// ��ȡָ��·���µ�InnerText
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xPath"></param>
        /// <param name="AttributeName"></param>
        /// <param name="IsThrowError">���û���ҵ��Ƿ��׳�����</param>
        /// <returns></returns>
        public static string GetAttribute(XmlDocument xml, string xPath,string AttributeName, bool IsThrowError)
        {
            XmlNode node = xml.SelectSingleNode(xPath);

            if (node == null && IsThrowError == true)
                throw new XmlToolsException(string.Format("û����Xml�ĵ����ҵ�·��Ϊ{0}�Ľڵ�", xPath));

            try
            {
                string attrText = node.Attributes[AttributeName].Value;

                if (string.IsNullOrEmpty(attrText) && IsThrowError == true)
                    throw new XmlToolsException(string.Format("û����·��Ϊ{0}�Ľڵ���û���ҵ�{1}������", xPath, AttributeName));

                return attrText;
            }
            catch (XmlToolsException) { throw; }
            catch (Exception ex)
            {
                if(IsThrowError)
                    throw new XmlToolsException(string.Format("û����·��Ϊ{0}�Ľڵ���û���ҵ�{1}������", xPath, AttributeName), ex);

                return string.Empty;
            }

            
        }

        /// <summary>
        /// ��ȡָ��·���µ�InnerText
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xPath"></param>
        /// <param name="IsThrowError">���û���ҵ��Ƿ��׳�����</param>
        /// <returns></returns>
        public static int GetInnerTextInt32(XmlDocument xml, string xPath, bool IsThrowError)
        {
            XmlNode node = xml.SelectSingleNode(xPath);

            if (node == null && IsThrowError == true)
                throw new XmlToolsException(string.Format("û����Xml�ĵ����ҵ�·��Ϊ{0}�Ľڵ�", xPath));

            string text = node.InnerText;

            int i = int.MinValue;

            if (!int.TryParse(text, out i) && IsThrowError == true)
            {
                throw new XmlToolsException(string.Format("·��Ϊ{0}�Ľڵ�{1}�޷�ת��ΪInt32����", xPath, text));
            }

            return i;
        }

        #endregion

        static string[] UnknownChars = new string[] { 
            "&#x0;", 
            "&#x1;", 
            "&#x2;", 
            "&#x3;", 
            "&#x4;", 
            "&#x5;", 
            "&#x6;", 
            "&#x7;", 
            "&#x8;", 
            "&#xB;", 
            "&#xC;", 
            "&#xE;", 
            "&#xF;", 
            "&#x00;", 
            "&#x01;", 
            "&#x02;", 
            "&#x03;", 
            "&#x04;", 
            "&#x05;", 
            "&#x06;", 
            "&#x07;", 
            "&#x08;", 
            "&#x0B;", 
            "&#x0C;", 
            "&#x0E;", 
            "&#x0F;", 
            "&#x10;", 
            "&#x11;", 
            "&#x12;", 
            "&#x13;", 
            "&#x14;", 
            "&#x15;", 
            "&#x16;", 
            "&#x17;", 
            "&#x18;", 
            "&#x19;", 
            "&#x1A;", 
            "&#x1B;", 
            "&#x1C;", 
            "&#x1D;", 
            "&#x1E;", 
            "&#x1F;", 
            "&#x20;" };

        #region ReplaceXmlString �滻�Ƿ��ַ�Ϊ�ո�

        /// <summary>
        /// �滻�Ƿ��ַ�Ϊ�ո�
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        static string ReplaceXmlString(string xml)
        {
            if (xml.IndexOf("&#x") < 0)
                return xml;

            StringBuilder sbXml = new StringBuilder(xml);

            foreach (string s in UnknownChars)
            {
                sbXml.Replace(s, string.Empty);
            }

            return sbXml.ToString();
        }

        #endregion

    }

    /// <summary>
    /// Xml�����쳣��
    /// </summary>
    public class XmlToolsException : Exception, ISerializable
    {
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="msg"></param>
        public XmlToolsException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public XmlToolsException(string msg, Exception ex)
            : base(msg, ex)
        {
        }
    }
}