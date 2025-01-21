package com.example.mobileagentcontrol

import android.os.Bundle
import android.widget.GridView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.google.firebase.firestore.FirebaseFirestore

class MainActivity : AppCompatActivity() {
    private lateinit var characterGrid: GridView
    private lateinit var db: FirebaseFirestore
    
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

        characterGrid = findViewById(R.id.characterGrid)
        val adapter = CharacterAdapter(this, characters)
        characterGrid.adapter = adapter

        characterGrid.setOnItemClickListener { _, _, position, _ ->
            val character = characters[position]
            sendCommand("select", character.x, character.y)
            Toast.makeText(this, "${character.name} seçildi", Toast.LENGTH_SHORT).show()
            
            // 1 saniye sonra kilitle
            android.os.Handler().postDelayed({
                sendCommand("lock", 950, 867)
                Toast.makeText(this, "Karakter kilitlendi", Toast.LENGTH_SHORT).show()
            }, 1000)
        }
    }

    private fun sendCommand(action: String, x: Int, y: Int) {
        val command = hashMapOf(
            "action" to action,
            "x" to x,
            "y" to y,
            "timestamp" to System.currentTimeMillis()
        )

        db.collection("commands")
            .add(command)
            .addOnSuccessListener {
                // Komut başarıyla gönderildi
            }
            .addOnFailureListener { e ->
                Toast.makeText(this, "Hata: ${e.message}", Toast.LENGTH_SHORT).show()
            }
    }
}