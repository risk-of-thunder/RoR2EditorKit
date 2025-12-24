using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UDebug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

namespace RoR2.Editor
{
    /// <summary>
    /// The RoR2EKLog is a custom logging class for the Package
    /// </summary>
    public static class RoR2EKLog
    {
        private enum LogLevel
        {
            Message,
            Debug,
            Warning,
            Error,
            Fatal
        }

        /// <summary>
        /// Logs a Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>, but will also display a dialog for the user to be aware of the message, if you don't want this popup, consider using <see cref="Debug(object)"/> instead.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Message(object message)
        {
            Log(LogLevel.Message, message, null);
        }

        /// <summary>
        /// Logs a Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>, but will also display a dialog for the user to be aware of the message, if you don't want this popup, consider using <see cref="Debug(object, UObject)"/> instead.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Message(object message, UObject context)
        {
            Log(LogLevel.Message, message, context);
        }

        /// <summary>
        /// Logs a Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>, but will also display a dialog for the user to be aware of the message, if you don't want this popup, consider using <see cref="Debug(object, UObject)"/> instead.
        /// </summary>
        /// <param name="format">A string that will be formatted utilizing the values in <paramref name="args"/></param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="args">Arguments for the string formatting procedure.</param>
        public static void Message(string format, UObject context, params object[] args)
        {
            Log(LogLevel.Message, string.Format(format, args), context);
        }

        /// <summary>
        /// Logs a Debug Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Debug(object message)
        {
            Log(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs a Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Debug(object message, UObject context)
        {
            Log(LogLevel.Debug, message, context);
        }

        /// <summary>
        /// Logs a Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.Log(object)"/>.
        /// </summary>
        /// <param name="format">A string that will be formatted utilizing the values in <paramref name="args"/></param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="args">Arguments for the string formatting procedure.</param>
        public static void Debug(string format, UObject context, params object[] args)
        {
            Log(LogLevel.Debug, string.Format(format, args), context);
        }

        /// <summary>
        /// Logs a Warning Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogWarning(object)"/>
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Warning(object message)
        {
            Log(LogLevel.Warning, message, null);
        }

        /// <summary>
        /// Logs a Warning Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogWarning(object)"/>
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Warning(object message, UObject context)
        {
            Log(LogLevel.Warning, message, context);
        }

        /// <summary>
        /// Logs a Warning Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogWarning(object)"/>
        /// </summary>
        /// <param name="format">A string that will be formatted utilizing the values in <paramref name="args"/></param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="args">Arguments for the string formatting procedure.</param>
        public static void Warning(string format, UObject context, params object[] args)
        {
            Log(LogLevel.Warning, string.Format(format, args), context);
        }


        /// <summary>
        /// Logs an Error Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Error(object message)
        {
            Log(LogLevel.Error, message, null);
        }

        /// <summary>
        /// Logs an Error Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Error(object message, UObject context)
        {
            Log(LogLevel.Error, message, context);
        }

        /// <summary>
        /// Logs an Error Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>
        /// </summary>
        /// <param name="format">A string that will be formatted utilizing the values in <paramref name="args"/></param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="args">Arguments for the string formatting procedure.</param>
        public static void Error(string format, UObject context, params object[] args)
        {
            Log(LogLevel.Error, string.Format(format, args), context);
        }

        /// <summary>
        /// Logs a Fatal Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>, but will also display a dialog for the user to be immediatly notified of the fatal exception. Afterwards, the editor will save any changes and close itself. As such it should only be used in situations where the editor will become highly unusable.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Fatal(object message)
        {
            Log(LogLevel.Fatal, message, null);
        }

        /// <summary>
        /// Logs a Fatal Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>, but will also display a dialog for the user to be immediatly notified of the fatal exception. Afterwards, the editor will save any changes and close itself. As such it should only be used in situations where the editor will become highly unusable.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Fatal(object message, UObject context)
        {
            Log(LogLevel.Fatal, message, context);
        }

        /// <summary>
        /// Logs a Fatal Message into the console.
        /// <br></br>
        /// This internally uses <see cref="UDebug.LogError(object)"/>, but will also display a dialog for the user to be immediatly notified of the fatal exception. Afterwards, the editor will save any changes and close itself. As such it should only be used in situations where the editor will become highly unusable.
        /// </summary>
        /// <param name="format">A string that will be formatted utilizing the values in <paramref name="args"/></param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="args">Arguments for the string formatting procedure.</param>
        public static void Fatal(string format, UObject context, params object[] args)
        {
            Log(LogLevel.Fatal, string.Format(format, args), context);
        }

        private static void Log(LogLevel logLevel, object data, UObject context)
        {
            string formattedMessage = Format(logLevel, data);
            switch (logLevel)
            {
                case LogLevel.Message:
                    UDebug.Log(formattedMessage, context);
                    EditorUtility.DisplayDialog($"Message", data.ToString(), "Ok");
                    break;
                case LogLevel.Debug:
                    UDebug.Log(formattedMessage, context);
                    break;
                case LogLevel.Warning:
                    UDebug.LogWarning(formattedMessage, context);
                    break;
                case LogLevel.Error:
                    UDebug.Log(formattedMessage, context);
                    break;
                case LogLevel.Fatal:
                    UDebug.LogError(formattedMessage, context);
                    EditorUtility.DisplayDialog("FATAL ERROR OCCURRED", string.Format("{0}\n\n{1}", formattedMessage, "The Editor will now save its assets then close"), "Ok");
                    AssetDatabase.SaveAssets();
                    EditorSceneManager.SaveOpenScenes();
                    EditorApplication.Exit(0);
                    break;
            }
        }

        private static string Format(LogLevel logLevel, object data)
        {
            return string.Format("[RoR2EditorKit--{0}]: {1}", logLevel, data);
        }
    }
}
