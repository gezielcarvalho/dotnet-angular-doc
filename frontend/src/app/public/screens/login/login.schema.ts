import z from 'zod';

export const loginSchema = z.object({
    username: z
        .string()
        .min(6, { message: 'Username must be at least 6 characters' }),
    password: z
        .string()
        .min(6, { message: 'Password must be at least 6 characters' }),
});
