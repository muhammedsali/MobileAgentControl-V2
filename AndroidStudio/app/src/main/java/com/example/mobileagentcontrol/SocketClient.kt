package com.example.mobileagentcontrol

import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.PrintWriter
import java.net.Socket

class SocketClient(private val serverIp: String, private val serverPort: Int = 12345) {
    private var socket: Socket? = null
    private var writer: PrintWriter? = null




    suspend fun sendCharacterSelect(character: Character) = withContext(Dispatchers.IO) {
        try {
            writer?.println("SELECT ${character.x} ${character.y}")
            Log.d("SocketClient", "Karakter seçim komutu gönderildi: ${character.name}")
        } catch (e: Exception) {
            Log.e("SocketClient", "Komut gönderme hatası: ${e.message}")
            disconnect()
            throw e
        }
    }

    suspend fun sendLockCommand(x: Int, y: Int) = withContext(Dispatchers.IO) {
        try {
            writer?.println("LOCK $x $y")
            Log.d("SocketClient", "Kilitleme komutu gönderildi")
        } catch (e: Exception) {
            Log.e("SocketClient", "Komut gönderme hatası: ${e.message}")
            disconnect()
            throw e
        }
    }

    fun disconnect() {
        try {
            writer?.close()
            socket?.close()
            Log.d("SocketClient", "Bağlantı kapatıldı")
        } catch (e: Exception) {
            Log.e("SocketClient", "Bağlantı kapatma hatası: ${e.message}")
        }
    }
}