using System;
using System.IO;
using System.Xml;

namespace Verse
{
	public class ScribeLoader
	{
		public CrossRefHandler crossRefs = new CrossRefHandler();

		public PostLoadIniter initer = new PostLoadIniter();

		public IExposable curParent;

		public XmlNode curXmlParent;

		public string curPathRelToParent;

		public void InitLoading(string filePath)
		{
			if (Scribe.mode != 0)
			{
				Log.Error("Called InitLoading() but current mode is " + Scribe.mode);
				Scribe.ForceStop();
			}
			if (this.curParent != null)
			{
				Log.Error("Current parent is not null in InitLoading");
				this.curParent = null;
			}
			if (this.curPathRelToParent != null)
			{
				Log.Error("Current path relative to parent is not null in InitLoading");
				this.curPathRelToParent = (string)null;
			}
			try
			{
				using (StreamReader input = new StreamReader(filePath))
				{
					using (XmlTextReader reader = new XmlTextReader(input))
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(reader);
						this.curXmlParent = xmlDocument.DocumentElement;
					}
				}
				Scribe.mode = LoadSaveMode.LoadingVars;
			}
			catch (Exception ex)
			{
				Log.Error("Exception while init loading file: " + filePath + "\n" + ex);
				this.ForceStop();
				throw;
				IL_00e7:;
			}
		}

		public void InitLoadingMetaHeaderOnly(string filePath)
		{
			if (Scribe.mode != 0)
			{
				Log.Error("Called InitLoadingMetaHeaderOnly() but current mode is " + Scribe.mode);
				Scribe.ForceStop();
			}
			try
			{
				using (StreamReader input = new StreamReader(filePath))
				{
					using (XmlTextReader xmlTextReader = new XmlTextReader(input))
					{
						if (!ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader))
							return;
						using (XmlReader reader = xmlTextReader.ReadSubtree())
						{
							XmlDocument xmlDocument = new XmlDocument();
							xmlDocument.Load(reader);
							XmlElement xmlElement = xmlDocument.CreateElement("root");
							xmlElement.AppendChild(xmlDocument.DocumentElement);
							this.curXmlParent = xmlElement;
						}
					}
				}
				Scribe.mode = LoadSaveMode.LoadingVars;
			}
			catch (Exception ex)
			{
				Log.Error("Exception while init loading meta header: " + filePath + "\n" + ex);
				this.ForceStop();
				throw;
				IL_00f6:;
			}
		}

		public void FinalizeLoading()
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Error("Called FinalizeLoading() but current mode is " + Scribe.mode);
			}
			else
			{
				try
				{
					Scribe.ExitNode();
					this.curXmlParent = null;
					this.curParent = null;
					this.curPathRelToParent = (string)null;
					Scribe.mode = LoadSaveMode.Inactive;
					this.crossRefs.ResolveAllCrossReferences();
					this.initer.DoAllPostLoadInits();
				}
				catch (Exception arg)
				{
					Log.Error("Exception in FinalizeLoading(): " + arg);
					this.ForceStop();
					throw;
					IL_0079:;
				}
			}
		}

		public bool EnterNode(string nodeName)
		{
			if (this.curXmlParent != null)
			{
				XmlNode xmlNode = this.curXmlParent[nodeName];
				if (xmlNode == null && char.IsDigit(nodeName[0]))
				{
					xmlNode = this.curXmlParent.ChildNodes[int.Parse(nodeName)];
				}
				if (xmlNode == null)
				{
					return false;
				}
				this.curXmlParent = xmlNode;
			}
			this.curPathRelToParent = this.curPathRelToParent + '/' + nodeName;
			return true;
		}

		public void ExitNode()
		{
			if (this.curXmlParent != null)
			{
				this.curXmlParent = this.curXmlParent.ParentNode;
			}
			if (this.curPathRelToParent != null)
			{
				int num = this.curPathRelToParent.LastIndexOf('/');
				if (num > 0)
				{
					this.curPathRelToParent = this.curPathRelToParent.Substring(0, num);
				}
				else
				{
					this.curPathRelToParent = (string)null;
				}
			}
		}

		public void ForceStop()
		{
			this.curXmlParent = null;
			this.curParent = null;
			this.crossRefs.Clear(false);
			this.initer.Clear();
			if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.ResolvingCrossRefs && Scribe.mode != LoadSaveMode.PostLoadInit)
				return;
			Scribe.mode = LoadSaveMode.Inactive;
		}
	}
}