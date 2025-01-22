package com.example.mobileagentcontrol

import android.os.Bundle
import android.util.Log
import android.widget.GridView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.google.firebase.firestore.FirebaseFirestore

class MainActivity : AppCompatActivity() {
    private lateinit var characterGrid: GridView
    private lateinit var db: FirebaseFirestore
    private val TAG = "MainActivity"
    
    private val characters = listOf(
        Character("Jett", R.drawable.jett, 622, 475),
        Character("Raze", R.drawable.raze, 958, 454),
        Character("Omen", R.drawable.omen, 1260, 445)
    )

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Firebase başlatma
        db = FirebaseFirestore.getInstance()
        Log.d(TAG, "Firebase başlatıldı")

        characterGrid = findViewById(R.id.characterGrid)
        val adapter = CharacterAdapter(this, characters)
        characterGrid.adapter = adapter

        characterGrid.setOnItemClickListener { _, _, position, _ ->
            val character = characters[position]
            Log.d(TAG, "${character.name} karakteri seçildi. X: ${character.x}, Y: ${character.y}")
            sendCommand("select", character.x, character.y)
            Toast.makeText(this, "${character.name} seçildi", Toast.LENGTH_SHORT).show()
            
            // 1 saniye sonra kilitle
            android.os.Handler().postDelayed({
                Log.d(TAG, "Kilitleme komutu gönderiliyor...")
                sendCommand("lock", 950, 867)
                Toast.makeText(this, "Karakter kilitlendi", Toast.LENGTH_SHORT).show()
            }, 1000)
        }
    }

    private fun sendCommand(action: String, x: Int, y: Int) {
        val command = hashMapOf(
            "action" to action,
            "x" to x.toLong(),
            "y" to y.toLong(),
            "timestamp" to System.currentTimeMillis()
        )

        Log.d(TAG, "Komut gönderiliyor: action=$action, x=$x, y=$y")

        db.collection("commands")
            .add(command)
            .addOnSuccessListener {
                Log.d(TAG, "Komut başarıyla gönderildi: $action, döküman ID: ${it.id}")
            }
            .addOnFailureListener { e ->
                Log.e(TAG, "Komut gönderme hatası: ${e.message}")
                Toast.makeText(this, "Hata: ${e.message}", Toast.LENGTH_SHORT).show()
            }
    }
}