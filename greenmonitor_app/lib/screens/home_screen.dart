import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  Map<String, dynamic>? _latestReading;
  List<dynamic> _readings = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);

    final latest = await ApiService.getLatestReading();
    final readings = await ApiService.getReadings();

    setState(() {
      _latestReading = latest;
      _readings = readings;
      _isLoading = false;
    });
  }

  Future<void> _logout() async {
    await ApiService.removeToken();
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('GreenMonitor'),
        backgroundColor: Colors.green,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: _logout,
          )
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadData,
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Поточні показники',
                        style: TextStyle(
                            fontSize: 18, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
                    if (_latestReading != null) ...[
                      Row(
                        children: [
                          Expanded(
                            child: _SensorCard(
                              icon: Icons.thermostat,
                              label: 'Температура',
                              value:
                                  '${_latestReading!['temperature']}°C',
                              color: Colors.orange,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: _SensorCard(
                              icon: Icons.water_drop,
                              label: 'Вологість',
                              value: '${_latestReading!['humidity']}%',
                              color: Colors.blue,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Оновлено: ${_latestReading!['timestamp'].toString().substring(0, 16).replaceAll('T', ' ')}',
                        style: const TextStyle(color: Colors.grey),
                      ),
                    ] else
                      const Text('Даних ще немає'),
                    const SizedBox(height: 24),
                    const Text('Історія',
                        style: TextStyle(
                            fontSize: 18, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
                    ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: _readings.length,
                      itemBuilder: (context, index) {
                        final r = _readings[index];
                        return Card(
                          child: ListTile(
                            leading: const Icon(Icons.eco,
                                color: Colors.green),
                            title: Text(
                                '🌡 ${r['temperature']}°C   💧 ${r['humidity']}%'),
                            subtitle: Text(r['timestamp']
                                .toString()
                                .substring(0, 16)
                                .replaceAll('T', ' ')),
                          ),
                        );
                      },
                    ),
                  ],
                ),
              ),
            ),
    );
  }
}

class _SensorCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color color;

  const _SensorCard({
    required this.icon,
    required this.label,
    required this.value,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Icon(icon, size: 40, color: color),
            const SizedBox(height: 8),
            Text(label, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 4),
            Text(value,
                style: const TextStyle(
                    fontSize: 24, fontWeight: FontWeight.bold)),
          ],
        ),
      ),
    );
  }
}