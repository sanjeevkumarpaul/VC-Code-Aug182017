using System;
using System.Reflection;
using System.IO;
using System.Xml;

using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;

namespace LogConfigurations
{
    public static class LoggingFactory
    {
         static LoggingFactory(){}
         
         public static ILogger Logger(Type t)
         {
             return new Logger( t.Name, t );
         }

         public static void Info(this ILogger log, string message)
         {
             var _log = log as Logger;
             
             log.Log( _log.Type , Level.Info, $"App Message - {message}", null);
         }

         public static void Critical(this ILogger log, string message)
         {
             var _log = log as Logger;
             
             log.Log( _log.Type , Level.Critical, $"App Critical - {message}", null);
         }

         public static void Error(this ILogger log, Exception ex)
         {
             var _log = log as Logger;
             
             log.Log( _log.Type , Level.Info, $"App Exception - {ex.Message}", ex);
         }
    }

    internal class Logger : ILogger
    {
        private string _name = string.Empty;
        private ILoggerRepository _loggerRepository = null;
        private ILog _log = null;
        private Type _type = null;
     
        public string Name { get { return _name; } }
        public Type Type { get {return _type; }}
        public ILoggerRepository Repository { get; }
        public bool IsEnabledFor(Level level){  return IsEnabled(level);  } 
        public void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception)
        {
            if (IsEnabled ( level ))
            {
                 if ( level == Level.Critical ) _log.Warn(message,exception);
                 else if (level == Level.Error) _log.Error(message, exception);                 
                 else if (level == Level.Info) _log.Info(message, exception);
            }
        }
        public void Log(LoggingEvent logEvent)
        {
            Log( this.GetType() , logEvent.Level, logEvent.MessageObject, logEvent.ExceptionObject );            
        }
        
        
        public Logger(string name, Type type)
        {    
            _name = name;   
            _type = type;
            _loggerRepository = log4net.LogManager.CreateRepository(    Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            _log = LogManager.GetLogger(_loggerRepository.Name, name);
            XmlConfigurator.Configure(_loggerRepository, new FileInfo("log4net.config"));
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        private bool IsEnabled(Level level)
        {
        
            if (level == Level.Critical):
                return _log.IsFatalEnabled;            
            if (level == Level.Debug || level == Level.Trace)
                return _log.IsDebugEnabled;            
            if (level == Level.Error)
                return _log.IsErrorEnabled;               
            if (level == Level.Information)
                return _log.IsInfoEnabled;
            if (level == Level.Warning)
                return _log.IsWarnEnabled;
           
           return false;
        }
       
    }
}
