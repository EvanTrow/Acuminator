using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionFile : IDisposable
	{
        private const string RootEmelent = "suppressions";
		public const string SuppressMessageElement = "suppressMessage";
		public const string SuppressionFileExtension = ".acuminator";

		internal string AssemblyName { get; }

		internal string Path { get; }

		private readonly ISuppressionFileWatcherService? _fileWatcher;

		/// <summary>
		/// Suppression work mode for this suppression file.
		/// </summary>
		internal GlobalSuppressionWorkMode SuppressionWorkMode { get; }

		private ConcurrentDictionary<SuppressMessage, object?> Messages { get; } = new ConcurrentDictionary<SuppressMessage, object?>();

		public HashSet<SuppressMessage> CopyMessages() => Messages.Keys.ToHashSet();

		public event FileSystemEventHandler Changed
		{
			add 
			{
				if (_fileWatcher != null)
				{
					_fileWatcher.Changed += value;
				}
			}
			remove
			{
				if (_fileWatcher != null)
				{
					_fileWatcher.Changed -= value;
				}
			}
		}

		private SuppressionFile(string assemblyName, string path, GlobalSuppressionWorkMode suppressionWorkMode,
								HashSet<SuppressMessage> messages, ISuppressionFileWatcherService? watcher)
		{
			AssemblyName = assemblyName;
			Path = path;
			SuppressionWorkMode = suppressionWorkMode;

			foreach (SuppressMessage message in messages)
			{
				Messages.TryAdd(message, null);
			}

			_fileWatcher = watcher;
		}

		internal bool ContainsMessage(SuppressMessage message) => Messages.ContainsKey(message);

		internal static SuppressionFile Load(ISuppressionFileSystemService fileSystemService, string suppressionFilePath,
											 GlobalSuppressionWorkMode suppressionWorkMode)
		{
			fileSystemService.ThrowOnNull();
			suppressionFilePath.ThrowOnNullOrWhiteSpace();

			string assemblyName = fileSystemService.GetFileName(suppressionFilePath).NullIfWhiteSpace() ??
								  throw new FormatException("Acuminator suppression file name cannot be empty");
			var messages = new HashSet<SuppressMessage>();

			if (!generateSuppressionBase)
			{
				messages = LoadMessages(fileSystemService, suppressionFilePath);
			}

			ISuppressionFileWatcherService? fileWatcher;

			lock (fileSystemService)
			{
				fileWatcher = fileSystemService.CreateWatcher(suppressionFilePath);
			}
			
			return new SuppressionFile(assemblyName, suppressionFilePath, suppressionWorkMode, messages, fileWatcher);
		}

		public void Dispose() => _fileWatcher?.Dispose();
		
		internal void AddMessage(SuppressMessage message) => Messages.TryAdd(message, null);

        public static XDocument NewDocumentFromMessages(IEnumerable<SuppressMessage> messages)
        {
            var root = new XElement(RootEmelent);
            var document = new XDocument(root);

            AddMessagesToDocument(document, messages);

            return document;
        }

		internal XDocument MessagesToDocument(ISuppressionFileSystemService fileSystemService)
		{
			fileSystemService.ThrowOnNull();
			XDocument? document;

			lock (fileSystemService)
			{
				document = fileSystemService.Load(Path);
			}

			if (document == null)
				throw new InvalidOperationException("Failed to open suppression file for edit");

			document.Root.RemoveNodes();
			AddMessagesToDocument(document, Messages.Keys);

			return document;
		}

		private static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages)
		{
			var comparer = new SuppressionMessageComparer();
			var sortedMessages = messages.OrderBy(m => m, comparer);

			foreach (var message in sortedMessages)
			{
				var xmlMessage = message.ToXml();

				if (xmlMessage != null)
					document.Root.Add(xmlMessage);
			}
		}

		public static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path)
		{
			XDocument? document;

			lock (fileSystemService)
			{
				document = fileSystemService.Load(path);
			}		

			if (document == null)
			{
				return [];
			}

			var suppressionMessages = new HashSet<SuppressMessage>();

			foreach (XElement suppressionMessageXml in document.Root.Elements(SuppressMessageElement))
			{
				SuppressMessage? suppressMessage = SuppressMessage.MessageFromElement(suppressionMessageXml);

				if (suppressMessage != null)
				{
					suppressionMessages.Add(suppressMessage.Value);
				}
			}

			return suppressionMessages;
		}
	}
}
