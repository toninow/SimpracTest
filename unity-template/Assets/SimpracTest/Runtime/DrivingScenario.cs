using System;

namespace SimpracTest
{
    [Serializable]
    public sealed class DrivingScenario
    {
        public string title;
        public string instruction;
        public string situation;
        public string[] answers;
        // 0 = correcta; 1 = leve; 2 = deficiente; 3 = eliminatoria.
        public int[] grades;

        public DrivingScenario(string title, string instruction, string situation, string[] answers, int[] grades)
        {
            this.title = title;
            this.instruction = instruction;
            this.situation = situation;
            this.answers = answers;
            this.grades = grades;
        }

        public static readonly DrivingScenario[] Demo =
        {
            new DrivingScenario("01 · Preparación",
                "Prepare el vehículo y salga cuando pueda hacerlo con seguridad.",
                "Está detenido al comienzo de un circuito didáctico.",
                new [] { "Ajusto espejos y asiento, me abrocho el cinturón y observo.",
                         "Arranco y salgo sin mirar.", "Inicio la marcha sin cinturón.",
                         "Me quedo parado sin motivo." },
                new [] { 0, 2, 3, 1 }),
            new DrivingScenario("02 · Marcha",
                "Continúe de frente por esta vía.",
                "Circula en segunda; la vía está despejada.",
                new [] { "Cambio suavemente a tercera.", "Reduzco de golpe a primera.",
                         "Acelero en exceso.", "Circulo en punto muerto." },
                new [] { 0, 2, 1, 2 }),
            new DrivingScenario("03 · STOP",
                "En el próximo cruce, gire a la derecha.",
                "STOP hipotético con visibilidad reducida.",
                new [] { "Me detengo; avanzo con precaución y vuelvo a mirar.",
                         "Paso despacio sin detenerme.", "Giro sin ver el tráfico.",
                         "Me detengo correctamente y espero sin necesidad." },
                new [] { 0, 3, 3, 1 }),
            new DrivingScenario("04 · Glorieta",
                "En la glorieta, tome la segunda salida.",
                "Hay tráfico circulando por la glorieta.",
                new [] { "Observo y cedo el paso antes de incorporarme.",
                         "Entro y obligo a otro coche a frenar.",
                         "Me detengo sin motivo pese a existir un hueco seguro.",
                         "Cambio de carril sin observar y fuerzo a otro vehículo." },
                new [] { 0, 3, 1, 3 }),
            new DrivingScenario("05 · Señalización",
                "En la siguiente calle, gire a la derecha.",
                "Una motocicleta circula próxima al carril derecho.",
                new [] { "Compruebo espejo y ángulo muerto; señalizo antes de cambiar.",
                         "Cambio de carril forzando a la motocicleta.",
                         "Cambio sin señalizar aunque hay espacio.",
                         "Me detengo sin motivo en el carril." },
                new [] { 0, 3, 1, 2 }),
            new DrivingScenario("06 · Estacionamiento",
                "Cuando pueda, estacione en un lugar adecuado.",
                "Hay una plaza libre. Maniobra representada de forma aproximada.",
                new [] { "Observo, señalizo y maniobro despacio.",
                         "Voy marcha atrás sin comprobar peatones.",
                         "Estaciono sin señalizar y sin afectar a nadie.",
                         "Obstaculizo el paso al aparcar." },
                new [] { 0, 3, 1, 2 })
        };
    }
}
