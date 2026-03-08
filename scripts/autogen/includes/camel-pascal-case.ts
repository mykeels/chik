/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-empty-object-type */

import { camelCase, pascalCase } from "change-case";

type CamelToPascalCase<S extends string> = S extends `${infer F}${infer R}`
  ? `${Capitalize<F>}${R}`
  : S;
type PascalToCamelCase<S extends string> = S extends `${infer F}${infer R}`
  ? `${Uncapitalize<F>}${R}`
  : S;

type StringKeys<TEntity extends {}> = {
  [key in keyof TEntity]: key extends string ? key : never;
}[keyof TEntity];
type IsLiteral<TValue> = TValue extends string | number | boolean | symbol
  ? true
  : false;

declare module "change-case" {
  export function camelCase<TInput extends string>(
    input: TInput,
  ): PascalToCamelCase<TInput>;
  export function pascalCase<TInput extends string>(
    input: TInput,
  ): CamelToPascalCase<TInput>;
}

export type ToCamel<TInput> = TInput extends string
  ? PascalToCamelCase<TInput>
  : IsLiteral<TInput> extends true
    ? TInput
    : TInput extends Array<infer TItem>
      ? Array<ToCamel<TItem>>
      : TInput extends { [key: string]: any }
        ? {
            [key in PascalToCamelCase<StringKeys<TInput>>]: IsLiteral<
              TInput[CamelToPascalCase<key>]
            > extends true
              ? TInput[CamelToPascalCase<key>]
              : ToCamel<TInput[CamelToPascalCase<key>]>;
          }
        : TInput;

/**
 * Converts a string, the keys of an object, or keys of an array of objects to camelCase.
 *
 * @example
 * toCamel('HelloWorld') // 'helloWorld'
 * toCamel({ HelloWorld: 'Hello World' }) // { helloWorld: 'Hello World' }
 * toCamel([{ HelloWorld: 'Hello World' }]) // [{ helloWorld: 'Hello World' }]
 */
export function toCamel<TInput>(
  input: TInput,
): TInput extends null | undefined ? TInput : ToCamel<TInput> {
  if (!input) {
    return input as any;
  } else if (Array.isArray(input)) {
    return input.map((i) => toCamel(i)) as any;
  } else if (typeof input === "string") {
    return camelCase(input) as any;
  } else if (typeof input === "object") {
    return Object.keys(input).reduce(
      (result, key) => {
        const literalTypes = ["string", "number", "boolean", "symbol"];
        const value = (input as Record<string, any>)[key];
        result[camelCase(key)] = literalTypes.includes(typeof value)
          ? value
          : toCamel(value);
        return result;
      },
      {} as Record<string, any>,
    ) as any;
  } else {
    return input as any;
  }
}

export type ToPascal<TInput> = TInput extends string
  ? CamelToPascalCase<TInput>
  : IsLiteral<TInput> extends true
    ? TInput
    : TInput extends Array<infer TItem>
      ? Array<ToPascal<TItem>>
      : TInput extends { [key: string]: any }
        ? {
            [key in CamelToPascalCase<StringKeys<TInput>>]: IsLiteral<
              TInput[PascalToCamelCase<key>]
            > extends true
              ? TInput[PascalToCamelCase<key>]
              : ToPascal<TInput[PascalToCamelCase<key>]>;
          }
        : TInput;

/**
 * Converts a string, the keys of an object, or keys of an array of objects to PascalCase.
 *
 * @example
 * toPascal('helloWorld') // 'HelloWorld'
 * toPascal({ helloWorld: 'Hello World' }) // { HelloWorld: 'Hello World' }
 * toPascal([{ helloWorld: 'Hello World' }]) // [{ HelloWorld: 'Hello World' }]
 */
export function toPascal<TInput>(
  input: TInput,
): TInput extends null | undefined ? TInput : ToPascal<TInput> {
  if (!input) {
    return input as any;
  } else if (Array.isArray(input)) {
    return input.map((i) => toPascal(i)) as any;
  } else if (typeof input === "string") {
    return pascalCase(input) as any;
  } else if (typeof input === "object") {
    return Object.keys(input).reduce(
      (result, key) => {
        const literalTypes = ["string", "number", "boolean", "symbol"];
        const value = (input as Record<string, any>)[key];
        result[pascalCase(key)] = literalTypes.includes(typeof value)
          ? value
          : toPascal(value);
        return result;
      },
      {} as Record<string, any>,
    ) as any;
  } else {
    return input as any;
  }
}

type Expect<T extends true> = T;

type Equal<X, Y> =
  (<T>() => T extends X ? 1 : 2) extends <T>() => T extends Y ? 1 : 2
    ? true
    : false;

// eslint-disable-next-line @typescript-eslint/no-unused-vars
type cases = [
  // Camel
  Expect<Equal<ToCamel<"HelloWorld">, "helloWorld">>,
  Expect<
    Equal<ToCamel<{ HelloWorld: "Hello World" }>, { helloWorld: "Hello World" }>
  >,
  Expect<
    Equal<
      ToCamel<[{ HelloWorld: "Hello World" }]>[0],
      { helloWorld: "Hello World" }
    >
  >,
  Expect<
    Equal<
      ToCamel<[{ HelloWorld: "Hello World" }, { HelloAfrica: "Hello Africa" }]>,
      (
        | {
            helloWorld: "Hello World";
          }
        | {
            helloAfrica: "Hello Africa";
          }
      )[]
    >
  >,
  // Pascal
  Expect<Equal<ToPascal<"helloWorld">, "HelloWorld">>,
  Expect<
    Equal<
      ToPascal<{ helloWorld: "Hello World" }>,
      { HelloWorld: "Hello World" }
    >
  >,
  Expect<
    Equal<
      ToPascal<[{ helloWorld: "Hello World" }]>[0],
      { HelloWorld: "Hello World" }
    >
  >,
  Expect<
    Equal<
      ToPascal<
        [{ helloWorld: "Hello World" }, { helloAfrica: "Hello Africa" }]
      >,
      (
        | {
            HelloWorld: "Hello World";
          }
        | {
            HelloAfrica: "Hello Africa";
          }
      )[]
    >
  >,
];
