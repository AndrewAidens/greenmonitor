import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'screens/login_screen.dart';
import 'screens/home_screen.dart';
import 'dart:io';
import 'services/api_service.dart';

/// Entry point. Disables SSL certificate validation for local development,
/// then launches the app.
void main() {
  HttpOverrides.global = MyHttpOverrides();
  runApp(const MyApp());
}

/// Root widget. Sets up the app theme and navigation.
class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'GreenMonitor',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.green),
        useMaterial3: true,
      ),
      home: const AuthWrapper(),
    );
  }
}

/// Checks whether a JWT token is stored on the device.
/// Redirects to HomeScreen if logged in, otherwise to LoginScreen.
class AuthWrapper extends StatefulWidget {
  const AuthWrapper({super.key});

  @override
  State<AuthWrapper> createState() => _AuthWrapperState();
}

class _AuthWrapperState extends State<AuthWrapper> {
  bool _isLoading = true;
  bool _isLoggedIn = false;

  @override
  void initState() {
    super.initState();
    _checkToken();
  }

 /// Reads the stored token from SharedPreferences to determine auth state.
  Future<void> _checkToken() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    setState(() {
      _isLoggedIn = token != null;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) return const Scaffold(
      body: Center(child: CircularProgressIndicator()),
    );
    return _isLoggedIn ? const HomeScreen() : const LoginScreen();
  }
}