import { createTheme } from '@mantine/core';

export const theme = createTheme({
  /* Colores del club — azul agua / verde pesca */
  primaryColor: 'blue',

  fontFamily: 'Inter, system-ui, -apple-system, sans-serif',

  defaultRadius: 'md',

  /* Componentes con overrides globales */
  components: {
    Button: {
      defaultProps: {
        size: 'sm',
      },
    },
    TextInput: {
      defaultProps: {
        size: 'sm',
      },
    },
    PasswordInput: {
      defaultProps: {
        size: 'sm',
      },
    },
    Select: {
      defaultProps: {
        size: 'sm',
      },
    },
  },
});
