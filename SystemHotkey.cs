﻿namespace QuickTool
 {
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

     /// <summary>
     /// Handles a System Hotkey
     /// </summary>
     public class SystemHotkey : Component  
     {
         public event EventHandler Pressed;
         public event EventHandler Error;

         public bool IsRegistered { set; get; }
 
         protected Keys hotKey=Keys.None;

         public SystemHotkey(IContainer container) {
             container.Add(this);

             NativeWindowWithEvent.Instance.ProcessMessage+=MessageEvent;
         }
 
         public SystemHotkey() {
             if (!DesignMode) {
                 NativeWindowWithEvent.Instance.ProcessMessage+=MessageEvent;
             }
         }
 
         protected override void Dispose(bool disposing){
             if (IsRegistered) {
                 UnregisterHotkey();
             }
             NativeWindowWithEvent.Instance.ProcessMessage-=MessageEvent;
             base.Dispose(disposing);
         }

         protected void MessageEvent(object sender,ref Message m,ref bool handled) {   
             if ((m.Msg==(int)Msgs.WM_HOTKEY)&&(m.WParam==(IntPtr)GetType().GetHashCode())){
                 handled=true;
                 
                 if (Pressed!=null) 
                     Pressed(this,EventArgs.Empty);
             }
         }
     
         protected bool UnregisterHotkey() {
             var result = false;
             if(IsRegistered)
                 result  = User32.UnregisterHotKey(NativeWindowWithEvent.Instance.Handle, this.GetType().GetHashCode());
             IsRegistered = false;
             return result;
         }
 
         protected bool RegisterHotkey(Keys key) {  
             Keys win32Key = key & ~(Keys.Alt|Keys.Control|Keys.Shift);
             Modifiers mod = ((key & Keys.Alt) != Keys.None ? Modifiers.MOD_ALT:0) | ((key & Keys.Shift) != Keys.None ? Modifiers.MOD_SHIFT:0) | ((key & Keys.Control) != Keys.None ? Modifiers.MOD_CONTROL:0);

             IsRegistered = User32.RegisterHotKey(NativeWindowWithEvent.Instance.Handle, GetType().GetHashCode(), (int)mod, (int)win32Key);

             return IsRegistered;
         }

         public Keys Shortcut {
             get { return hotKey; }
             set {
                 hotKey=value;

                 if(DesignMode) {
                     return; //Don't register in Designmode
                 }

                 if((IsRegistered)&&(hotKey!=value))  //Unregister previous registered Hotkey
                 {
                     if(!UnregisterHotkey()) {
                         if(Error!=null)
                             Error(this, EventArgs.Empty);
                     }
                 }

                 if(value!=Keys.None) {
                     if(!RegisterHotkey(value)) {
                         if(Error != null)
                             Error(this, EventArgs.Empty);
                     }
                 }
             }
         }
     }
 }
