﻿using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Pdf
{
	public class PdfDictionary : PdfObject
	{
		public Dictionary<string, PdfItem> Items { get; set; } = new();

		public override string GetStringForm()
		{
			StringBuilder str = new StringBuilder();

			str.Append(this.getStartObj());
			str.Append(this.getBody());
			str.Append(this.getEndObj());

			return str.ToString();
		}

		protected string getStartObj()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"% {this.GetType().FullName}");

			sb.AppendLine($"{this.Id} 0 obj");

			return sb.ToString();
		}

		protected string getBody()
		{
			StringBuilder str = new StringBuilder();

			str.AppendLine("<<");
			foreach (var item in this.Items)
			{
				str.AppendLine($"{item.Key} {item.Value.GetStringForm()}");
			}
			str.AppendLine(">>");

			return str.ToString();
		}

		protected string getEndObj()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(PdfKey.EndObj);
			sb.AppendLine(PdfKey.CommentSeparator);

			return sb.ToString();
		}
	}
}
