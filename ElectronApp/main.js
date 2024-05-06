const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');

console.log("main js loaded");

let cpuSpeed = 0.0;

let arg1value = app.commandLine.getSwitchValue("url");

const createWindow = () => {
    console.log("create window starting...");

    const win = new BrowserWindow({
        width: 1600,
        height: 1200,
    });
    
    console.log("url is: " + arg1value);

    win.loadURL(arg1value);
}

app.whenReady().then(() => {

    createWindow();

    app.on('activate', () => {
        if(BrowserWindow.getAllWindows().length === 0) createWindow();
    });
});

app.on('window-all-closed', () => {
    if(process.platform !== 'darwin') app.quit();
});