﻿using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Pdf.Extensions;
using ACadSharp.Tables;
using CSMath;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.IO;

namespace ACadSharp.Pdf
{
	/// <summary>
	/// Exporter to create a pdf document.
	/// </summary>
	public class PdfExporter : IDisposable
	{
		/// <summary>
		/// Notification event to get information about the export process.
		/// </summary>
		/// <remarks>
		/// The notification system informs about any issue or non critical errors during the export.
		/// </remarks>
		public event NotificationEventHandler OnNotification;

		public PdfExporterConfiguration Configuration { get; } = new PdfExporterConfiguration();

		public PdfDocumentInformation Information
		{
			get { return this._pdf.Info; }
		}

		private readonly PdfDocument _pdf;

		/// <summary>
		/// Initialize an instance of <see cref="PdfExporter"/>.
		/// </summary>
		/// <param name="path">Path where the pdf will be saved.</param>
		public PdfExporter(string path) : this(File.Create(path))
		{
		}

		/// <summary>
		/// Initialize an instance of <see cref="PdfExporter"/>.
		/// </summary>
		/// <param name="stream">Stream where the pdf will be saved.</param>
		public PdfExporter(Stream stream)
		{
			this._pdf = new PdfDocument(stream);
		}

		public void AddPage(Layout layout)
		{
			if (!layout.IsPaperSpace)
			{
				return;
			}

			PdfPage page = this._pdf.AddPage();
			page.Size = PdfSharp.PageSize.A0;

			foreach (Viewport vp in layout.Viewports)
			{

			}

			throw new NotImplementedException();
		}

		public void AddPage(BlockRecord block)
		{
			PdfPage page = this._pdf.AddPage();

			BoundingBox viewBox = BoundingBox.Null;
			XGraphics gfx = XGraphics.FromPdfPage(page);
			gfx.PageDirection = XPageDirection.Upwards;

			foreach (Entity e in block.Entities)
			{
				XPen pen = getDrawingPen(e);

				this.drawEntity(gfx, pen, e);
				viewBox = viewBox.Merge(e.GetBoundingBox());
			}

			updateSize(page, gfx, viewBox);
		}

		public void AddPage(BlockRecord block, PlotSettings settings)
		{
			throw new NotImplementedException();
		}

		public void AddPages(CadDocument document)
		{
			foreach (Objects.Layout layout in document.Layouts)
			{
				this.AddPage(layout);
			}
		}

		/// <summary>
		/// Close the document and save it.
		/// </summary>
		public void Close()
		{
			this._pdf.Close();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_pdf.Dispose();
		}

		private XPen getDrawingPen(Entity entity)
		{
			Color color;
			if (entity.Color.IsByLayer)
			{
				color = entity.Color;
			}
			else
			{
				color = entity.Color;
			}

			return new XPen(color.ToXColor(), 1);
		}

		protected void notify(string message, NotificationType notificationType, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, ex));
		}

		private void drawEntity(XGraphics gfx, XPen pen, Entity entity)
		{
			switch (entity)
			{
				case Arc arc:
					this.drawArc(gfx, pen, arc);
					break;
				case Circle circle:
					this.drawCircle(gfx, pen, circle);
					break;
				case Line line:
					this.drawLine(gfx, pen, line);
					break;
				case Point point:
					this.drawPoint(gfx, pen, point);
					break;
				default:
					this.notify($"Entity {entity.SubclassMarker} not implemented.", NotificationType.NotImplemented);
					break;
			}
		}

		private void drawArc(XGraphics gfx, XPen pen, Arc arc)
		{
			gfx.DrawArc(pen, arc.GetBoundingBox().ToXRect(), MathUtils.RadToDeg(arc.StartAngle), MathUtils.RadToDeg(arc.EndAngle));
		}

		private void drawCircle(XGraphics gfx, XPen pen, Circle circle)
		{
			var box = circle.GetBoundingBox();
			gfx.DrawEllipse(pen, box.ToXRect());
		}

		private void drawLine(XGraphics gfx, XPen pen, Line line)
		{
			gfx.DrawLine(pen, line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
		}

		private void drawPoint(XGraphics gfx, XPen pen, Point line)
		{
			//Not working
			gfx.DrawLine(pen, line.Location.X, line.Location.Y, line.Location.X, line.Location.Y);
		}

		private void updateSize(PdfPage page, XGraphics gfx, BoundingBox box)
		{
			if (box.Extent == BoundingBoxExtent.Infinite ||
				box.Extent == BoundingBoxExtent.Null)
			{
				return;
			}

			page.Width = new XUnit(box.Max.X);
			page.Height = new XUnit(box.Max.Y);

			//gfx.TranslateTransform(box.Min.X, box.Max.Y);
		}
	}
}
